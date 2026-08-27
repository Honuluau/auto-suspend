public class HttpClientHouse {
    private static HttpClient httpClient = new HttpClient();

    /// <summary>
    /// Retrieves static HttpClient.
    /// </summary>
    /// <returns>Static HttpClient among all classes.</returns>
    public static HttpClient GetHttpClient() {
        return httpClient;
    }

    /// <summary>
    /// This method is only called once by Auto-Suspend and it updates the HttpClient.
    /// </summary>
    /// <param name="newHttpClient"></param>
    public static void SetHttpClient(HttpClient newHttpClient) {
        httpClient = newHttpClient;
    }
}