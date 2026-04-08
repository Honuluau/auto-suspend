using System.Text.Json;

public class LoansAPI
{
    public static int TotalRequests = 0;

    public static async Task<UserLoansAPI?> GetLoansByEagleId(HttpClient httpClient, string eagleId)
    {
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{eagleId}/loans?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
        try
        {
            string jsonString = await httpClient.GetStringAsync(url);
            UserLoansAPI loans = JsonSerializer.Deserialize<UserLoansAPI>(jsonString)!;
            TotalRequests++;
            Logger<LoansAPI>.Log($"Grabbed loans for {eagleId}", LogLevel.Info);

            return loans;
        }
        catch (Exception e)
        {
            Logger<LoansAPI>.Log($"An error occured when looking up loans for eagle id: {e.Message}", LogLevel.Error);
            return null; // switch to null
        }
    }

    public static async Task<int> GetLoansForEagleIds(HttpClient httpClient, List<string> eagleIds)
    {
        UserLoansAPI? loans = await GetLoansByEagleId(httpClient, "901458044");
        if (loans != null)
        {
            foreach (LoanAPI loan in loans.item_loan)
            {
                
            }
        }
        return 0;
    }
}