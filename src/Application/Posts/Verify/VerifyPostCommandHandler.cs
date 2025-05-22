using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.Verify;

internal sealed class VerifyPostCommandHandler(
    IPostRepository postRepository,
    IPostCounterRepository postCounterRepository,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<VerifyPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(VerifyPostCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        
        if (post is null )
        {
            return Result.Failure<Guid>(new Error("400", "Invalid otp", ErrorType.Validation));
        }

        if (post.OtpExpirationTime < dateTimeProvider.UtcNow)
        {
            return Result.Failure<Guid>(new Error("400", "Otp expired", ErrorType.Validation));
        }

        post.Title = command.Title;
        post.Description = command.Description;
        post.MobilNumbers = command.MobileNumbers;
        post.TransactionMode = command.TransactionMode;
        post.PaymentType = command.PaymentType;
        post.Amount = command.Amount;
        post.Status = nameof(Status.Pending);
        
        await postRepository.UpdateAsync(post);
        await postCounterRepository.IncrementAsync("Posts", 1);

        return post.Id;
    }
}
