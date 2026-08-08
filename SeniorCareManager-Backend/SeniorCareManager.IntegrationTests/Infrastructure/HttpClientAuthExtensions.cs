namespace SeniorCareManager.IntegrationTests.Infrastructure;

public static class HttpClientAuthExtensions
{
    public static HttpClient AsUser(this HttpClient client, Guid userId)
    {
        client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        return client;
    }
}
