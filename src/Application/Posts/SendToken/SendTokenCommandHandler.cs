using System.Text;
using Application.Abstractions;
using Application.Abstractions.Messaging;
using Domain.Posts;
using SharedKernel;

namespace Application.Posts.SendToken;

internal class SendTokenCommandHandler(
    IPostRepository postRepository,
    ISmsSender smsSender)
    : ICommandHandler<SendTokenCommand, bool>
{
    public async Task<Result<bool>> Handle(SendTokenCommand command, CancellationToken cancellationToken)
    {
        Post? post = await postRepository.GetByIdAsync(command.PostId);

        if (post is null)
        {
            return Result.Failure<bool>(PostErrors.NotFound(command.PostId));
        }


        var otpString = new StringBuilder();

        int otp = OptGenerator.GenerateOtpCode();
        otpString.Append($"Dhoka.io verification code is: {otp}\n");
        otpString.Append("This code is valid for 5 minutes. Never share your OTP with anyone.\n");
        otpString.Append("Thank you for helping build a scam free Bangladesh.");

        bool iSendSms = await smsSender.SendSms(command.PhoneNumber, otpString.ToString());

        if (!iSendSms)
        {
            return Result.Success(false);
        }

        post.Otp = otp;
        post.ContactNumber = command.PhoneNumber;
        post.OtpExpirationTime = DateTime.UtcNow.AddMinutes(10);

        await postRepository.UpdateAsync(post);
        return Result.Success(true);

    }
}
