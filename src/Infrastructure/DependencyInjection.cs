using System.Text;
using System.Threading.RateLimiting;
using Amazon;
using Amazon.CloudFront;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Application.Abstractions.Authentication;
using Domain.Comments;
using Domain.Contacts;
using Domain.Posts;
using Domain.Tokens;
using Domain.Users;
using Infrastructure.Authentication;
using Infrastructure.BackgroundServices;
using Infrastructure.Comments;
using Infrastructure.Contacts;
using Infrastructure.ImageServices;
using Infrastructure.MessageServices;
using Infrastructure.Posts;
using Infrastructure.Tokens;
using Infrastructure.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenSearch.Client;
using SharedKernel;


namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddAwsServices(configuration)
            .AddDatabase(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {

        services.AddHttpClient<ISmsSender, SmsService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.WithOrigins("https://www.dhoka.io")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
        });

        // Add rate limiter service
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5, // 5 requests
                        Window = TimeSpan.FromSeconds(10), // per 10 seconds
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    }));

            options.RejectionStatusCode = 429;
        });

        services.AddMemoryCache();
        services.AddHostedService<DailyJobService>();
        services.AddSingleton<PasswordHasher>();
        services.AddScoped<TokenProvider>();

        services.AddHostedService<DailyJobService>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var credentials = new BasicAWSCredentials(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"]);
        services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(credentials, RegionEndpoint.APSoutheast1));

        services.AddSingleton<IOpenSearchClient>(sp =>
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            string? url = configuration["AWS:OpenSearch:Url"];
            string? username = configuration["AWS:OpenSearch:Username"];
            string? password = configuration["AWS:OpenSearch:Password"];
            ConnectionSettings? settings = new ConnectionSettings(new Uri(url))
                .BasicAuthentication(username, password)
                .DefaultIndex("dhoka-data-index")
                .DefaultFieldNameInferrer(p => p);

            return new OpenSearchClient(settings);
        });

        services.AddScoped<IDynamoDBContext, DynamoDBContext>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IPostCounterRepository, PostCounterRepository>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenProvider, TokenProvider>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        return services;
    }


    private static IServiceCollection AddAwsServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IAmazonS3>(sp =>
        {
            string? region = config["AWS:Region"];
            string? accessKey = config["AWS:AccessKey"];
            string? secretKey = config["AWS:SecretKey"];
            return new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.APSoutheast1);
        });

        services.AddSingleton<IAmazonCloudFront>(sp =>
        {
            string? region = config["AWS:Region"];
            string? accessKey = config["AWS:AccessKey"];
            string? secretKey = config["AWS:SecretKey"];
            return new AmazonCloudFrontClient(accessKey, secretKey, Amazon.RegionEndpoint.APSoutheast1);
        });

        services.AddSingleton<IImageService, S3Service>();

        return services;
    }
}
