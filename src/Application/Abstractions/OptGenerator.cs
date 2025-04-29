using System.Security.Cryptography;

namespace Application.Abstractions;

public class OptGenerator
{
    public static int GenerateOtpCode()
    {
        byte[] randomNumber = new byte[2];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        int value = BitConverter.ToUInt16(randomNumber, 0) % 9000 + 1000;
        return value;
    }
}
