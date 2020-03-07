using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PageScrapeSos
{
    public static class UpdateCandidates
    {
        #region Properties

        #endregion

        #region Variables

        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string OfficeSearchResultsUrl = "https://elections.sos.ga.gov/GAElection/CandidateDetails";

        private static readonly NameValueCollection HiddenArguments = new NameValueCollection();

        private static HttpResponseMessage _httpRespMsg;

        public static readonly ScrapeStatus CurrentStatus = new ScrapeStatus
        {
            TotalPages = -1,
            LastPageCompleted = 0,
            ScrapeComplete = false,
            LastOpMessage = "Static initializer",
            LoggingOn = true,
            InternalLoggingOn = true
        };

        public static long BytesReceived { get; set; }


        #endregion Variables

        #region Constants

        #endregion

        private static void ResetStatus(bool loggingOn, bool intLoggingOn)
        {
            HiddenArguments.Clear();

            CurrentStatus.TotalPages = -1;
            CurrentStatus.TotalCandidates = -1;
            CurrentStatus.LastPageCompleted = 0;
            CurrentStatus.ScrapeComplete = false;
            CurrentStatus.LoggingOn = false;  // avoid output on last msg chg
            CurrentStatus.LastOpMessage = string.Empty;
            CurrentStatus.LoggingOn = true;
            CurrentStatus.SbLog.Clear();
            CurrentStatus.InternalLoggingOn = intLoggingOn;
            CurrentStatus.LoggingOn = loggingOn;
        }

        public static ScrapeStatus GetElections(FormSearchSos formSearchSos)
        {
            // TODO: put exception catcher here for network problems.
            var contentString = PostIt(new Uri(OfficeSearchResultsUrl), formSearchSos).Result;

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"GetElections call returned Status Code: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return CurrentStatus;
            }

            if (string.IsNullOrEmpty(contentString))
            {
                CurrentStatus.LastOpMessage = "GetElections received null content";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return CurrentStatus;
            }

            CurrentStatus.LastOpMessage = "GetElections received document length = " + contentString.Length;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contentString);

            const string tgtSelect = "//select[@id='id_election']/option";

            var electNodes = htmlDoc.DocumentNode.SelectNodes(tgtSelect);

            if (electNodes == null)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastOpMessage = "ElectNodes search returned null.";
                return CurrentStatus;
            }

            var elections = electNodes.Select(node => new Election(node.Attributes[0].Value, node.InnerText)).ToList();

            CurrentStatus.ScrapeComplete = true;
            CurrentStatus.ScrapeSuccess = true;
            CurrentStatus.Elections = elections;
            
            return CurrentStatus;
        }

        public static ScrapeStatus GetCandidates(FormSearchSos formSearchSos)
        {
            var contentString = PostIt(new Uri(OfficeSearchResultsUrl), formSearchSos).Result;

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"GetCandidates call returned Status Code: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return CurrentStatus;
            }

            if (string.IsNullOrEmpty(contentString))
            {
                CurrentStatus.LastOpMessage = "GetCandidates received null content";
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.LastPageCompleted++;
                return CurrentStatus;
            }

            CurrentStatus.LastOpMessage = "GetCandidates received document length = " + contentString.Length;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(contentString);
            var year = formSearchSos.ElectionYear;

            var candList = new List<CandidateSos>();

            const string tgtDiv = "//*[@class=\"col1Inner\"]/table/tr";
            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtDiv);

            if (nodes == null)
            {
                CurrentStatus.ScrapeComplete = true;
                CurrentStatus.ScrapeSuccess = false;
                CurrentStatus.LastOpMessage = "Data table search returned null.";
                return CurrentStatus;
            }

            var candDesc = string.Empty;

            foreach (var nodetr in nodes)
            {
                var tdObj = nodetr.ChildNodes[0];  // td  1

                if (tdObj.Attributes.Count > 0 && tdObj.Attributes[0].Name == "colspan")  // Account for #text children here
                {
                    // Start of candidate section
                    candDesc = CleanUpWhiteSpace(tdObj.InnerText);
                }
                else
                {
                    // Get candidates 
                    if (nodetr.InnerHtml.Contains("Qualified - Signatures Required"))
                    {
                        // We're at the end of the list, asterisk 
                        break;
                    }
                    var subdoc = new HtmlDocument();
                    subdoc.LoadHtml(nodetr.InnerHtml);
                    var rowcntr = 1;

                    // November fails here

                    var candTrRows = from table in subdoc.DocumentNode.SelectNodes("//table")
                                     from row in table.SelectNodes("tr")
                                     from cell in row.SelectNodes("td")
                                     select new CellData { RowNum = rowcntr++, CellText = CleanUpWhiteSpace(cell.InnerText) };

                    candList.Add(FillCandidate(candTrRows.ToList(), candDesc, year.VarValue));

                }
            }

            CurrentStatus.ScrapeComplete = true;
            CurrentStatus.ScrapeSuccess = true;
            CurrentStatus.Candidates = candList;
            return CurrentStatus;
        }


        public static CandidateSos FillCandidate(List<CellData> cellData, string officeName, string year)
        {
            var candidate = new CandidateSos()
            {
                OfficeName = officeName,
                Year = year
            };


            foreach (var data in cellData)
            {
                var cellTextLen = data.CellText.Length;

                // get labeled text first

                if (data.CellText.Length < 5)
                {
                    // "N/A" or other junk
                    continue;
                }

                switch (data.CellText.Substring(0, 5))
                {
                    case "E-mai":
                        candidate.Email = "Blocked";
                        break;

                    case "INCUM":
                        if (cellTextLen > 11)
                        {
                            candidate.Incumbent = data.CellText.Remove(0, 11);
                        }
                        break;

                    case "OCCUP":
                        if (cellTextLen > 12)
                        {
                            candidate.Occupation = data.CellText.Remove(0, 12);
                        }
                        break;

                    case "QUALI":
                        if (cellTextLen > 16)
                        {
                            candidate.QualifiedDate = data.CellText.Remove(0, 16);
                        }
                        break;

                    case "PARTY":
                        if (cellTextLen > 7)
                        {
                            candidate.Party = data.CellText.Remove(0, 7);
                        }
                        break;

                    case "PHONE":
                        if (cellTextLen > 14)
                        {
                            candidate.PhoneNumber = data.CellText.Remove(0, 14);
                        }
                        break;

                    case "WEBSI":
                        if (cellTextLen > 9)
                        {
                            candidate.Website = data.CellText.Remove(0, 9);
                        }

                        break;

                    default:

                        switch (data.RowNum)
                        {
                            case 1:
                                candidate.CandidateName = data.CellText;
                                break;

                            case 2:
                                candidate.Address = data.CellText;
                                break;

                            case 3:
                                candidate.CityStZip = data.CellText;
                                break;

                            default:

                            break;      // Inner default

                        }               // End Inner switch
                    break;              // Outer default
                }                       // End Outer switch
            }

            return candidate;
        }

        public static string CleanUpWhiteSpace(string desc)
        {
            return desc.Trim().Replace("\r\n", " ").Replace("\t", string.Empty);
        }

        private static async Task<string> PostIt(Uri uri, FormSearchSos formSearchSos)
        {
            var formContent = new FormUrlEncodedContent(formSearchSos.FormDataList());

            // PrintKeyValuePairs(formDataList);

            var request = new HttpRequestMessage {RequestUri = uri, Method = HttpMethod.Post, Content = formContent};

            SetRequestHeaders(request);

            _httpRespMsg = await NetHttpClient.Client.SendAsync(request);

            if (!_httpRespMsg.IsSuccessStatusCode)
            {
                CurrentStatus.LastOpMessage = $"PostIt could not retrieve URL, StatusCode: {_httpRespMsg.StatusCode}";
                CurrentStatus.ScrapeComplete = true;
                return string.Empty;
            }

            var stringContent = await _httpRespMsg.Content.ReadAsStringAsync();
            BytesReceived += stringContent.Length;

            return stringContent;
        }

        private static void SetRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Origin", "http://media.ethics.ga.gov");
            request.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
            request.Headers.Add("X-MicrosoftAjax", "Delta=true");
        }

        public static void PrintKeysAndValues(NameValueCollection myCol)
        {
            foreach (string s in myCol.AllKeys)
            {
                Log.Debug($"{s} {myCol[s]}");
            }
        }

        public static void PrintKeyValuePairs(List<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                Log.Debug($"KVP {pair.Key}, {pair.Value}");
            }

        }
    }
}
/*
 * log4net levels:
All – Log everything.
Debug.
Info.
Warn.
Error.
Fatal.
Off – Don't log anything.
Feb 9, 2017
 */
