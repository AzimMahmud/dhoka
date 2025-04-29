using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using SharedKernel;


namespace Infrastructure.MessageServices;


public class SmsService : ISmsSender
{
    private readonly HttpClient _client;
    private readonly string? apiUrl;
    private readonly string? apiKey;

    public SmsService(IConfiguration config, HttpClient client)
    {
        _client = client;
        apiUrl = config["SMS:API_Url"];
        apiKey = config["SMS:API_Key"];
    }

    public Task<bool> SendSms(string toNumber, string message)
    {
        _client.BaseAddress = new Uri(apiUrl); // //
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        HttpResponseMessage response = _client.GetAsync("?api_key=" + apiKey + "&msg=" + message + "&to=" + toNumber)
            .Result;

        if (response.IsSuccessStatusCode)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
