using System.Threading.Tasks;

public class AlmaLoan
{
    
}

public class UserFulfillment
{
    private static async Task<AlmaLoan?> RequestLoan(HttpClient httpClient, string loanAlmaId, string userPrimaryIdentifier)
    {
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{userPrimaryIdentifier}/loans/{loanAlmaId}&apikey={SensitiveInfo.DevelopmentServerAPIKey}";
        Console.WriteLine(url);

        try
        {
            string xmlData = await httpClient.GetStringAsync(url);

            File.AppendAllText("loan_example.txt", xmlData);

            return null;
        }
        catch (Exception e)
        {
            Logger<UserFulfillment>.Error($"An error occured while requesting loan ({loanAlmaId}) for ({userPrimaryIdentifier}).", e);
            return null;
        }
    }

    public static async Task<AlmaLoan?> SearchLoan(string loanAlmaId, string userPrimaryIdentifier)
    {
        HttpClient httpClient = HttpClientHouse.GetHttpClient();
        AlmaLoan? loan = await RequestLoan(httpClient, loanAlmaId, userPrimaryIdentifier);

        return loan;
    }
}