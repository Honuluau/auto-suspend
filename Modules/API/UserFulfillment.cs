using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

/// <summary>
/// This class represents all of the data that Auto-Suspend uses from a loan provided by Alma.
/// </summary>
public class AlmaLoan {
    // User
    public string UserPrimaryIdentifier { get; set; }

    // Item
    public string Barcode { get; set; }
    public string ItemPolicy { get; set; }

    // Loan
    public string CircDesk { get; set; }
    public string DueDate { get; set; }
    public string LibraryName { get; set; }
    public string LoanDate { get; set; }
    public string LoanId { get; set; }
    public string? ReturnCircDesk { get; set; }
    public string? ReturnDate { get; set; }

    /// <summary>
    /// Constructor method, every parameter is self-explanatory.
    /// </summary>
    public AlmaLoan(string userPrimaryIdentifier, string barcode, string itemPolicy, string loanId,
        string circDesk, string? returnCircDesk, string libraryName, string dueDate, string loanDate,
        string? returnDate) {
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

    /// <returns>True if a loan has a valid return date.</returns>
    public bool IsReturned() {
        return ReturnDate != null;
    }
}

public class UserFulfillment {
    private static int _totalRequests = 0;

    /// <summary>
    /// This method requests the loan via a private method.
    /// </summary>
    /// <param name="loanAlmaId">The in-house id for a loan in Alma.</param>
    /// <param name="userPrimaryIdentifier">The primary identifier for a patron in Alma.</param>
    /// <returns>An alma loan is it exists.</returns>
    public static async Task<AlmaLoan?> SearchLoan(string loanAlmaId, string userPrimaryIdentifier) {
        HttpClient httpClient = HttpClientHouse.GetHttpClient();
        AlmaLoan? loan = await RequestLoan(httpClient, loanAlmaId, userPrimaryIdentifier);

        return loan;
    }

    /// <summary>
    /// This method requests a loan using the API.
    /// </summary>
    /// <param name="httpClient">HttpClient.</param>
    /// <param name="loanAlmaId">The id for the loan relative to Alma.</param>
    /// <param name="userPrimaryIdentifier">The primary identifer for the user.</param>
    /// <returns>An AlmaLoan if the loan is valid.</returns>
    private static async Task<AlmaLoan?> RequestLoan(HttpClient httpClient, string loanAlmaId,
        string userPrimaryIdentifier) {
        // Form the length API url.
        StringBuilder urlBuilder = new StringBuilder(SensitiveInfo.GetUserDetailsUrl);
        urlBuilder.Append(userPrimaryIdentifier);
        urlBuilder.Append("/loans/");
        urlBuilder.Append(loanAlmaId);
        urlBuilder.Append("?apikey=");
        urlBuilder.Append(SensitiveInfo.DevelopmentServerAPIKey);
        string url = urlBuilder.ToString();

        try {
            string xmlData = await httpClient.GetStringAsync(url);
            _totalRequests++;

            // THE RETURN DATES ARE IN UTC ZULU TIME. NOT THE LOCAL TIME ZONE.

            XDocument document = XDocument.Parse(xmlData);

            // Parsing information
            XElement barcode = document.Root!.Element("item_barcode")!;
            XElement circDesk = document.Root!.Element("circ_desk")!;
            XElement dueDate = document.Root!.Element("due_date")!;
            XElement itemPolicy = document.Root!.Element("item_policy")!;
            XElement libraryName = document.Root!.Element("library")!;
            XElement loanDate = document.Root!.Element("loan_date")!;
            XElement loanId = document.Root!.Element("loan_id")!;
            XElement? returnDate = document.Root!.Element("return_date");
            XElement userPrimaryIdentifierXML = document.Root!.Element("user_id")!;

            // For some unknown reason, Alma always includes this in the XML but keeps it empty.
            XElement returnCircDesk = document.Root!.Element("return_circ_desk")!;

            // Creating the Alma Loan.
            string? returnCircDeskString = null;
            string? returnDateString = null;

            if (returnCircDesk.Value != null) {
                returnCircDeskString = returnCircDesk.Value;
            }

            if (returnDate != null) {
                returnDateString = returnDate.Value;
            }

            AlmaLoan loan = new AlmaLoan(userPrimaryIdentifierXML.Value, barcode.Value, itemPolicy.Value, 
                loanId.Value, circDesk.Value, returnCircDeskString, libraryName.Value, dueDate.Value, 
                loanDate.Value, returnDateString);

            return loan;
        }
        catch (Exception e) {
            StringBuilder errorBuilder = new StringBuilder("An error occured while requesting loan (");
            errorBuilder.Append(loanAlmaId);
            errorBuilder.Append(") for (");
            errorBuilder.Append(userPrimaryIdentifier);
            errorBuilder.Append(").");

            Logger<UserFulfillment>.Error(errorBuilder.ToString(), e);
            return null;
        }
    }
}