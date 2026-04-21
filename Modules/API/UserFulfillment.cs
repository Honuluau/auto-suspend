using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml.Linq;

public class AlmaLoan
{
    // User
    public string UserPrimaryIdentifier { get; set; }
    
    // Item
    public string Barcode { get; set; }
    public string ItemPolicy { get; set; }

    // Loan
    public string LoanId { get; set; }
    public string CircDesk { get; set; }
    public string? ReturnCircDesk { get; set; }
    public string LibraryName { get; set; }
    public string DueDate { get; set; }
    public string LoanDate { get; set; }
    public string? ReturnDate { get; set; }

    public AlmaLoan(string userPrimaryIdentifier, string barcode, string itemPolicy, string loanId,
        string circDesk, string? returnCircDesk, string libraryName, string dueDate, string loanDate, string? returnDate)
    {
        this.UserPrimaryIdentifier = userPrimaryIdentifier;
        this.Barcode = barcode;
        this.ItemPolicy = itemPolicy;
        this.LoanId = loanId;
        this.CircDesk = circDesk;
        this.ReturnCircDesk = returnCircDesk;
        this.LibraryName = libraryName;
        this.DueDate = dueDate;
        this.LoanDate = loanDate;
        this.ReturnDate = returnDate;
    }

    public bool IsReturned()
    {
        return (ReturnDate != null);
    }
}

public class UserFulfillment
{
    private static int totalRequests = 0;

    private static async Task<AlmaLoan?> RequestLoan(HttpClient httpClient, string loanAlmaId, string userPrimaryIdentifier)
    {
        string url = $"{SensitiveInfo.GetUserDetailsUrl}{userPrimaryIdentifier}/loans/{loanAlmaId}?apikey={SensitiveInfo.DevelopmentServerAPIKey}";
        Console.WriteLine(url);

        try
        {
            string xmlData = await httpClient.GetStringAsync(url);
            totalRequests++;

            // THE RETURN DATES ARE IN UTC ZULU TIME. NOT THE LOCAL TIME ZONE.

            XDocument document = XDocument.Parse(xmlData);

            // Parsing information
            XElement loanId = document.Root!.Element("loan_id")!;
            XElement circDesk = document.Root!.Element("circ_desk")!;
            XElement returnCircDesk = document.Root!.Element("return_circ_desk")!; // For some unknown reason, Alma always includes this in the XML but keeps it empty.
            XElement libraryName = document.Root!.Element("library")!;
            XElement userPrimaryIdentifierXML = document.Root!.Element("user_id")!;
            XElement barcode = document.Root!.Element("item_barcode")!;
            XElement dueDate = document.Root!.Element("due_date")!;
            XElement loanDate = document.Root!.Element("loan_date")!;
            XElement? returnDate = document.Root!.Element("return_date");
            XElement itemPolicy = document.Root!.Element("item_policy")!;


            // Creating the Alma Loan.
            string? returnCircDeskString = null;
            string? returnDateString = null;

            if (returnCircDesk.Value != null)
            {
                returnCircDeskString = returnCircDesk.Value;
            }

            if (returnDate != null)
            {
                returnDateString = returnDate.Value;
            }

            AlmaLoan loan = new AlmaLoan(userPrimaryIdentifierXML.Value, barcode.Value, itemPolicy.Value, loanId.Value, circDesk.Value,
                returnCircDeskString, libraryName.Value, dueDate.Value, loanDate.Value, returnDateString);

            return loan;
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