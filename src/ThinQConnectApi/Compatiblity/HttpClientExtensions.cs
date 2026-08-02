namespace System.Net.Http;

internal static class HttpClientExtensions
{
#if NETSTANDARD2_0
    public static Task<string> GetStringAsync(this HttpClient httpClient, string requestUri, CancellationToken _)
    {
        return httpClient.GetStringAsync(requestUri);
    }
#endif
}