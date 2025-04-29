namespace SharedKernel;

public interface ISmsSender
{
    Task<bool> SendSms(string toNumber, string message);
}
