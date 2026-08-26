public class ReturnSystem
{
    // This method returns the current loans with missing return dates that do not appear in the overdue report.
    private static List<Loan> GetMissingLoansFromOverdue(Loan[] sqlOverdueLoans)
    {
        List<Loan> missingLoans = new List<Loan>();

        foreach (Loan loan in sqlOverdueLoans)
        {
            // Checks Overdues for a matching loan based off item barcode and loan date because one item cannot be loaned out at the exact same time.
            Overdue? correspondingOverdue = OverdueAnalytics.Overdues.Find(o => o.Barcode == loan.Item.Barcode && ParseDates.ConvertStringToDateTime(o.LoanDate) == loan.LoanDate);

            // If there is a corresponding overdue, then it has not been returned yet. So we only get a list of ones that have been returned.
            if (correspondingOverdue == null)
            {
                missingLoans.Add(loan);
            }
        }

        return missingLoans;
    }

    public static int ProcessMissingOverdues()
    {
        Loan[]? sqlOverdueLoans = SQLInterface.GetAllNonReturnedLoans();
        if (sqlOverdueLoans == null)
        {
            Logger<ReturnSystem>.Log("SQL Overdue Loans list is null.", LogLevel.Error);
            return 24;
        }

        // Loans that have been returned in Alma but do not have a date marked.
        List<Loan> missingLoans = GetMissingLoansFromOverdue(sqlOverdueLoans);

        // Somehow get access to the RETURN_DATE of a loan. I know that I knew of a method 3 months ago, I'll have to figure it out again.

        return 0;
    }
}