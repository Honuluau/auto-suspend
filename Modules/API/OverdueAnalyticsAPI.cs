using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.Linq;

public class OverdueAnalyticsAPI
{
    private static List<string> EagleIds = new List<string>();
    private static int TotalRequests = 0;
    private static XNamespace ROWSET = "urn:schemas-microsoft-com:xml-analysis:rowset";

    private static async Task<List<string>> GetPageOfEagleIds(HttpClient httpClient, string? resumptionToken)
    {
        string? newResumptionToken = null;

        string url = $"{SensitiveInfo.OverdueReportUrl}&apikey={SensitiveInfo.AnalyticsAPIKey}";
        if (resumptionToken != null)
        {
            url = $"{url}&token={resumptionToken}";
        }

        List<string> eagleIds = new List<string>();

        try
        {
            /* 
            // I don't intend on spamming the live server so this is commented out during development. Data is written to a txt file in bin.

            string xmlData = await httpClient.GetStringAsync(url);
            TotalRequests++;
            */

            string xmlData = File.ReadAllText("overduexml.txt");
            XDocument document = XDocument.Parse(xmlData);

            // Get Resumption Token
            XElement queryResult = document.Root!.Element("QueryResult")!;
            XElement isFinishedElement = queryResult.Element("IsFinished")!;
            if (isFinishedElement.Value == "false")
            {
                XElement resumptionTokenElement = queryResult.Element("ResumptionToken")!;
                newResumptionToken = resumptionTokenElement.Value;
            }

            // Get Eagle Ids
            var rows = document.Descendants(ROWSET + "Row");
            foreach (XElement row in rows)
            {
                string? eagleId = row.Element(ROWSET + "Column4")?.Value;
                
                if (eagleId != null)
                {
                    eagleIds.Add(eagleId);
                } else
                {
                    Logger<OverdueAnalyticsAPI>.Log($"An eagle id is NULL in the xml return data.", LogLevel.Error);
                    return ["FAIL"];
                }
            }
        }
        catch (Exception e)
        {
            Logger<OverdueAnalyticsAPI>.Log($"Failed to Get Page of Eagle Ids: {e.Message}", LogLevel.Error);
            return ["FAIL"];
        }

        // If Resumption Token was found.
        if (newResumptionToken != null)
        {
            eagleIds.Add(newResumptionToken);
            eagleIds.Add("ResumptionToken");
        }

        return eagleIds;
    }

    public static async Task<int> GatherOverdueEagleIds(HttpClient httpClient)
    {
        // You just got done writing the Page request. Create a loop to gather all eagleIds. You check if there is a resumption token by checking the last index.
        List<string> eagleIds = await GetPageOfEagleIds(httpClient, null);
        if (eagleIds.Count == 1)
        {
            if (eagleIds[0] == "FAIL")
            {
                return 13;
            }
        }

        foreach (string eagleId in eagleIds)
        {
            Console.WriteLine(eagleId);
        }

        return 0;
    }
}