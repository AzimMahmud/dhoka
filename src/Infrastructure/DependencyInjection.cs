using System.Text;
using Amazon;
using Amazon.CloudFront;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.S3;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Comments;
using Domain.Posts;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Comments;
using Infrastructure.Database;
using Infrastructure.ImageServices;
using Infrastructure.MessageServices;
using Infrastructure.Posts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenSearch.Client;
using SharedKernel;
using DateTimeProvider = Infrastructure.Time.DateTimeProvider;
using IDateTimeProvider = SharedKernel.IDateTimeProvider;


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
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal(configuration)
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddHttpClient<ISmsSender, SmsService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader();
        }));

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
        });

        services.AddMemoryCache();


        services.AddSingleton<PasswordHasher>();
        services.AddScoped<TokenProvider>();


        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

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

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("Database")!);

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

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

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
