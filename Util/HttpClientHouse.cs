public class HttpClientHouse
{
    private static HttpClient httpClient = new HttpClient();

    public static void SetHttpClient(HttpClient newHttpClient)
    {
        httpClient = newHttpClient;
    }

    public static HttpClient GetHttpClient()
    {
        return httpClient;
    }
}