using System.Globalization;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.Extensions.Caching.Memory;
using SharedKernel;
using Status = Domain.Posts.Status;

namespace Application.Posts.Verify;

internal sealed class VerifyPostCommandHandler(
    IPostRepository postRepository,
    IPostCounterRepository postCounterRepository,
    IMemoryCache cache)
    : ICommandHandler<VerifyPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(VerifyPostCommand command, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(command.ContactNumber, out _))
        {
            return Result.Failure<Guid>(
                new Error("429",
                    "You have already submitted a report recently. You can submit another report every 20 minutes.",
                    ErrorType.Validation)
            );
        }

        Post post = await postRepository.GetByIdAsync(command.PostId);

        if (post.Id == Guid.Empty)
        {
            return Result.Failure<Guid>(new Error("400", "Invalid otp", ErrorType.Validation));
        }

        if (post.Otp != command.Otp)
        {
            return Result.Failure<Guid>(new Error("400", "Invalid otp", ErrorType.Validation));
        }

        if (post.OtpExpirationTime < DateTime.UtcNow)
        {
            return Result.Failure<Guid>(new Error("400", "Otp expired", ErrorType.Validation));
        }

        

        post.ScamType = command.ScamType;
        post.Title = command.Title;
        post.PaymentType = command.PaymentType;
        post.Description = command.Description;
        post.MobileNumbers = command.MobileNumbers;
        post.Amount = command.Amount;
        post.PaymentDetails = command.PaymentDetails;
        post.ScamDateTime = command.ScamDateTime;
        post.AnonymityPreference = command.AnonymityPreference;
        post.Name = command.Name;
        post.ContactNumber = command.ContactNumber;
        post.Status = nameof(Status.Pending);

        Task postTask = postRepository.UpdateAsync(post);
        Task postCountIncreaseTask = postCounterRepository.IncrementAsync("Posts", 1);

        await Task.WhenAll(postTask, postCountIncreaseTask);

        cache.Set(
            command.ContactNumber,
            command.ContactNumber,
            TimeSpan.FromMinutes(20)
        );

        return post.Id;
    }
}
