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

        public Guid Id { get; set; }
    
        public string ScamType { get; set; }

  
        public string Title { get; set; }
        
        
        public string PaymentType { get; set; }
        
        public List<string> MobileNumbers { get; set; } = new List<string>();
        
        public decimal Amount { get; set; }

        public string PaymentDetails { get; set; }

        public DateTime ScamDateTime { get; set; }
        
        public string AnonymityPreference { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }
        public string ContactNumber { get; set; }
        
        public int Otp { get; set; }
        
    }


    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("posts/{id:guid}/verify", async (Guid id, Request request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new VerifyPostCommand
            {
                PostId = id,
                ScamType = request.ScamType,
                Otp = request.Otp,
                Title = request.Title,
                Amount = request.Amount,
                PaymentDetails = request.PaymentDetails,
                ScamDateTime = request.ScamDateTime,
                AnonymityPreference = request.AnonymityPreference,
                Description = request.Description,
                ContactNumber = request.ContactNumber,
                Name = request.Name,
                MobileNumbers = request.MobileNumbers,
                PaymentType = request.PaymentType,
            };

            Result<Guid> result = await sender.Send(command, cancellationToken);
            
            

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Posts);
    }
}
