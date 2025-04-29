using Application.Posts.Verify;
using MediatR;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Posts;

internal sealed class Verify : IEndpoint
{
    public sealed class Request
    {
        public int Otp { get; set;}
        public string Title { get; set;}
        public string TransactionMode { get; set;}
        public string PaymentType { get; set;}
        public string Description { get; set;}
        public List<string> MobileNumbers { get; set; } = [];
        public decimal Amount { get; set; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("posts/{id:guid}/verify", async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new VerifyPostCommand
            {
                PostId = id,
                Otp = request.Otp,
                Title = request.Title,
                TransactionMode = request.TransactionMode,
                PaymentType = request.PaymentType,
                Description = request.Description,
                MobileNumbers = request.MobileNumbers,
                Amount = request.Amount
            };

            Result<Guid> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Posts);
    }
}
