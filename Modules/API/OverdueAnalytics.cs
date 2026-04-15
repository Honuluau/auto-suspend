using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;

public class Overdue // Derived information from the custom fulfillment report created by Justin.
{
    // Patron information.
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserGroup { get; set; }
    public string UserPrimaryIdentifier { get; set; }

    // Item information
    public string Barcode { get; set; }
    public string Title { get; set; }

    // Loan information.
    public string CircDesk { get; set; }
    public string ItemLoanId { get; set; }
    public string ItemPolicy { get; set; } // Policy may change in the future so it is important to count it with the Loan.
    public string LibraryName { get; set; }
    public string PreferredEmail { get; set; } // Same thought-process as policy.
    public string LoanDate { get; set; }
    public string DueDate { get; set; }

    public Overdue(string firstName, string lastName, string userGroup, string userPrimaryIdentifier, string barcode, string title,
    string circDesk, string itemLoadId, string itemPolicy, string libraryName, string preferredEmail, string loanDate, string dueDate)
    {
        this.FirstName = firstName;
        this.LastName = lastName;
        this.UserGroup = userGroup;
        this.UserPrimaryIdentifier = userPrimaryIdentifier;
        this.Barcode = barcode;
        this.Title = title;
        this.CircDesk = circDesk;
        this.ItemLoanId = itemLoadId;
        this.ItemPolicy = itemPolicy;
        this.LibraryName = libraryName;
        this.PreferredEmail = preferredEmail;
        this.LoanDate = loanDate;
        this.DueDate = dueDate;
    }
}

public class OverdueAnalytics
{
    private static XNamespace ROWSET = "urn:schemas-microsoft-com:xml-analysis:rowset";
    private static int totalRequests = 0;

    public static List<Overdue> Overdues = new List<Overdue>();

    private static async Task<string?> GetPageOfOverdues(HttpClient httpClient, string? resumptionToken)
    {
        string? newResumptionToken = null; // Default to no resumption.

        // Format URL for request.
        string url = $"{SensitiveInfo.CustomOverdueReportUrl}&apikey={SensitiveInfo.AnalyticsAPIKey}";
        if (resumptionToken != null)
        {
            url = $"{url}&token={resumptionToken}";
        }

        try
        {
            // Retrieve XML data
            string xmlData = await httpClient.GetStringAsync(url);
            totalRequests++;

            XDocument document = XDocument.Parse(xmlData);

            // Get Resumption Token if it exists.
            XElement queryResult = document.Root!.Element("QueryResult")!;
            XElement isFinishedElement = queryResult.Element("IsFinished")!;
            if (isFinishedElement.Value == "false")
            {
                XElement resumptionTokenELement = queryResult.Element("ResumptionToken")!;
                newResumptionToken = resumptionTokenELement.Value;
            }

            // Add overdues to list.
            var rows = document.Descendants(ROWSET + "Row");
            foreach (XElement row in rows)
            {
                // Ugh. There has got to be a better way.
                string firstName = row.Element(ROWSET + "Column1")!.Value;
                string lastName = row.Element(ROWSET + "Column2")!.Value;
                string userGroup = row.Element(ROWSET + "Column3")!.Value;
                string userPrimaryIdentifier = row.Element(ROWSET + "Column4")!.Value;
                string circDesk = row.Element(ROWSET + "Column5")!.Value;
                string barcode = row.Element(ROWSET + "Column6")!.Value;
                string itemLoadId = row.Element(ROWSET + "Column7")!.Value;
                string title = row.Element(ROWSET + "Column9")!.Value;
                string itemPolicy = row.Element(ROWSET + "Column10")!.Value;
                string libraryName = row.Element(ROWSET + "Column11")!.Value;
                string preferredEmail = row.Element(ROWSET + "Column12")!.Value;
                string dueDate = row.Element(ROWSET + "Column13")!.Value;
                string loanDate = row.Element(ROWSET + "Column14")!.Value;

                Overdue overdue = new Overdue(firstName, lastName, userGroup, userPrimaryIdentifier, barcode, title, circDesk, itemLoadId, itemPolicy, libraryName, preferredEmail, loanDate, dueDate);
                Overdues.Add(overdue);
            }
        }
        catch (Exception e)
        {
            Logger<OverdueAnalytics>.Error("Failed to get page of overdues", e);
            return "FAIL";
        }

        Logger<OverdueAnalytics>.Log($"Retrieved {Overdues.Count} overdues so far.", LogLevel.Info);
        return newResumptionToken;
    }

    public static async Task<int> GatherOverdueAnalytics(HttpClient httpClient)
    {
        string? resumptionToken = null;

        Logger<OverdueAnalytics>.Log($"Reading overdue analytics through API and writing to database.", LogLevel.Info);
        Stopwatch.Start();

        // Get pages of overdues by iterating with the resumption token.
        while (true)
        {
            string? newResumptionToken = await GetPageOfOverdues(httpClient, resumptionToken);
            if (newResumptionToken == null) {
                break;
            }
            else if (newResumptionToken == "Fail")
            {
                return 13;
            }
            else
            {
                resumptionToken = newResumptionToken;
            }
        }

        Logger<OverdueAnalytics>.Log($"Successfully gathered ({Overdues.Count}) overdue analytics in {Stopwatch.Stop()}.", LogLevel.Info);
        return 0;
    }

    public static int GetTotalRequests()
    {
        return totalRequests;
    }
}