using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.Linq;

public class OverdueAnalyticsAPI
{
    private static List<string> EagleIds = new List<string>();
    private static int TotalRequests = 0;
    private static XNamespace ROWSET = "urn:schemas-microsoft-com:xml-analysis:rowset";

    // This method requests Alma Analytics API for a page (50) of eagle ids. It will append the resumptionToken to the end of the list it returns.
    private static async Task<List<string>> GetPageOfEagleIds(HttpClient httpClient, string? resumptionToken)
    {
        string? newResumptionToken = null;

        // Formatting the url to contain the request URL, API Key, and Resumption Token.
        string url = $"{SensitiveInfo.OverdueReportUrl}&apikey={SensitiveInfo.AnalyticsAPIKey}";
        if (resumptionToken != null)
        {
            url = $"{url}&token={resumptionToken}";
        }

        List<string> eagleIds = new List<string>();

        try
        {
            // Retrieve XML Data.
            string xmlData = await httpClient.GetStringAsync(url);
            TotalRequests++;

            XDocument document = XDocument.Parse(xmlData);

            // Get Resumption Token
            XElement queryResult = document.Root!.Element("QueryResult")!;
            XElement isFinishedElement = queryResult.Element("IsFinished")!;
            if (isFinishedElement.Value == "false")
            {
                XElement resumptionTokenElement = queryResult.Element("ResumptionToken")!;
                newResumptionToken = resumptionTokenElement.Value;
            }

            // Get Eagle Ids per row in XML.
            var rows = document.Descendants(ROWSET + "Row");
            foreach (XElement row in rows)
            {
                string? eagleId = row.Element(ROWSET + "Column4")?.Value;

                if (eagleId != null)
                {
                    eagleIds.Add(eagleId);
                }
                else
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
            eagleIds.Add($"ResumptionToken-{newResumptionToken}");
        }

        return eagleIds;
    }

    // This function updates the static class by getting all pages of eagle ids from the overdue analytics API. 
    public static async Task<int> GatherOverdueEagleIds(HttpClient httpClient)
    {
        List<string> eagleIds = new List<string>();
        string? resumptionToken = null;

        // Get pages, check for resumption token, if there is one: continue else stop.
        Stopwatch.Start();
        Logger<OverdueAnalyticsAPI>.Log("Gathering eagle ids with overdue loans start.", LogLevel.Info);
        while (true)
        {
            List<string> page = await GetPageOfEagleIds(httpClient, resumptionToken);
            if (page.Count == 1)
            {
                if (page[0] == "FAIL")
                {
                    return 13;
                }
            }
            else if (page.Count == 0)
            {
                break;
            }

            string lastElement = page[page.Count - 1];
            if (lastElement.Contains("ResumptionToken"))
            {
                resumptionToken = lastElement.Substring(16);
                page.RemoveAt(page.Count - 1);
            }

            foreach (string eagleId in page)
            {
                // Do not add duplicate eagle ids.
                if (!eagleIds.Contains(eagleId))
                {
                    eagleIds.Add(eagleId);
                }
            }

            // Has to be after the addition of eagle ids.
            if (!lastElement.Contains("ResumptionToken"))
            {
                break;
            }
        }

        EagleIds = eagleIds;
        Logger<OverdueAnalyticsAPI>.Log($"Gathering finished successfully in {Stopwatch.Stop()}. Found {EagleIds.Count} unique eagle ids with {TotalRequests} API calls.", LogLevel.Info);
        
        return 0;
    }

    public static List<string> GetEagleIds()
    {
        return EagleIds;
    }
}