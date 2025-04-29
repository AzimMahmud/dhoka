using Application.Abstractions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Posts;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Posts.SendToken;

internal class SendTokenCommandHandler(
    IApplicationDbContext context,
    IDateTimeProvider dateTimeProvider,
    ISmsSender smsSender)
    : ICommandHandler<SendTokenCommand, int>
{
    public async Task<Result<int>> Handle(SendTokenCommand command, CancellationToken cancellationToken)
    {
        Post? post = await context.Posts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.PostId, cancellationToken: cancellationToken);

        if (post is null)
        {
            return Result.Failure<int>(PostErrors.NotFound(command.PostId));
        }

        try
        {
            int otp = OptGenerator.GenerateOtpCode();
            bool iSendSms = true;//await smsSender.SendSms(command.PhoneNumber, $"Your otp is {otp}");

            if (iSendSms)
            {
                post.Otp = otp;
                post.ContactNumber = command.PhoneNumber;
                post.OtpExpirationTime = dateTimeProvider.UtcNow.AddMinutes(10);
                context.Posts.Update(post);
                await context.SaveChangesAsync(cancellationToken);
            }
            return Result.Success(otp);
        }
        catch(Exception ex) 
        {
            return Result.Success(0);
        }
    }
}
