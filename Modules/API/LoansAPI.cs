using System.Text.Json;

public class LoansAPI
{
    public static int TotalRequests = 0;

    public static async Task<UserLoansAPI?> GetLoansByEagleId(HttpClient httpClient, string eagleId)
    {
        string jsonString = "";
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{eagleId}/loans?apikey={SensitiveInfo.DevelopmentServerAPIKey}&format=json";
        try
        {
            jsonString = await httpClient.GetStringAsync(url);
            UserLoansAPI loans = JsonSerializer.Deserialize<UserLoansAPI>(jsonString)!;
            TotalRequests++;
            Logger<LoansAPI>.Log($"Grabbed loans for {eagleId}", LogLevel.Info);

            return loans;
        }
        catch (Exception e)
        {
            Logger<LoansAPI>.Log($"An error occured when looking up loans for eagle id: {e.Message}", LogLevel.Error);
            File.WriteAllText($"/missing loans/{eagleId}.txt", jsonString);
            return null; // switch to null
        }
    }

    public static async Task<int> GetLoansForEagleIds(HttpClient httpClient, List<string> eagleIds)
    {
        Logger<LoansAPI>.Log($"Retrieving Loans through API.. Expecting {eagleIds.Count} requests...", LogLevel.Info);
        Stopwatch.Start();
        
        foreach (string eagleId in eagleIds)
        {
            try
            {
                UserLoansAPI? loans = await GetLoansByEagleId(httpClient, eagleId);
                if (loans != null && loans.item_loan != null)
                {
                    foreach (LoanAPI loan in loans.item_loan)
                    {
                        // Insert a shell of an item to SQL based off data.
                        SQLItemInterface.InsertItem(loan.mms_id, loan.item_barcode, loan.title, loan.description, loan.item_policy.value!);
                        int itemId = SQLItemInterface.GetItemId(loan.item_barcode);
                        int patronId = SQLPatronInterface.GetPatronId(eagleId);
                        if (itemId > 0 && patronId > 0)
                        {
                            // Insert Loan handles if the loan already exists. No need for duplication checking here.
                            SQLLoanInterface.InsertLoan(loan.loan_id, loan.circ_desk.value!, patronId, itemId, loan.loan_date, loan.due_date);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger<LoansAPI>.Log($"An error occured while getting loans for {eagleId} in a list. {e.Message}", LogLevel.Error);
                return 18;
            }
        }

        Logger<LoansAPI>.Log($"Finished retrieving loans in {Stopwatch.Stop()} with {TotalRequests} API requests.", LogLevel.Info);
        return 0;

    }
}