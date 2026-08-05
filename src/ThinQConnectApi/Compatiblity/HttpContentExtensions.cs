namespace System.Net.Http;

internal static class HttpContentExtensions
{
#if NETSTANDARD2_0
    public static Task<string> ReadAsStringAsync(this HttpContent httpContent, CancellationToken _)
    {
        return httpContent.ReadAsStringAsync();
    }
#endif
}