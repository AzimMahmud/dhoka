using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.Verify;

internal sealed class VerifyPostCommandHandler(
    IPostRepository postRepository,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<VerifyPostCommand, Guid>
{
    public async Task<Result<Guid>> Handle(VerifyPostCommand command, CancellationToken cancellationToken)
    {
        Post? post =  await postRepository.GetByIdAsync(command.PostId);
        
       
        // Post? post = await context.Posts.FirstOrDefaultAsync(x =>
        //     x.Id == command.PostId && x.Otp == command.Otp, cancellationToken: cancellationToken);

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
        
        // await context.SaveChangesAsync(cancellationToken);

        return post.Id;
    }
}
