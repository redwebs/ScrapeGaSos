using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace PageScrapeSos
{
    public static class ScrapHelp
    {
        public static string RemoveLine(string inStr)
        {
            return inStr.Substring(inStr.IndexOf("\r\n", StringComparison.Ordinal));
        }

        public static Tuple<int, int> GetCurrentPageAndCount(string footer)
        {
            /* Showing <span class="paging">1 </span>     of 
            <span class="paging">3</span> Pages  */

            var footerSub = footer.Substring(footer.IndexOf("paging", StringComparison.Ordinal) + 8);
            var currentPgNum = Convert.ToInt32(footerSub.Substring(0, footerSub.IndexOf("<", StringComparison.Ordinal) - 1));

            var footerLine2 = RemoveLine(footer);
            var start = footerLine2.IndexOf(">", StringComparison.Ordinal) + 1; 
            var end = footerLine2.IndexOf("</span", StringComparison.Ordinal);

            var totalPagesStr = footerLine2.Substring(start, end - start);
            var totalPages = Convert.ToInt32(totalPagesStr);

            return new Tuple<int, int>(currentPgNum, totalPages);
        }

        public static StringBuilder TestAgility(string html)
        {
            var candList = new List<CandidateSos>();

            var sb = new StringBuilder();
            var theHtml =
                System.IO.File.ReadAllText(@"C:\Users\fred\Dropbox\VoterGuide\ScrapeSos\3_CandidateDetailsMar24-2.html");

            // declare object of HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(theHtml);

            const string tgtDiv = "//*[@class=\"col1Inner\"]/table//tr";
            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtDiv);

            if (nodes == null)
            {
                //CurrentStatus.ScrapeComplete = true;
                //CurrentStatus.LastOpMessage = "Data table search returned null.";
                //return false;
            }

            var candDesc = string.Empty;

            foreach (var nodetr in nodes)
            {
                var tdObj = nodetr.ChildNodes[1];  // td

                if (tdObj.Attributes[1].Name == "colspan")
                {
                    // Start of candidate section
                    candDesc = CleanUpWhiteSpace(tdObj.InnerText);
                    sb.Append("Candidate - ");
                    sb.AppendLine(candDesc);
                }
                else
                {
                    // Get candidates 
                    var subdoc = new HtmlDocument();
                    subdoc.LoadHtml(nodetr.InnerHtml);
                    var rowcntr = 1;

                    var candTrRows = from table in subdoc.DocumentNode.SelectNodes("//table")
                        from row in table.SelectNodes("tr")
                        from cell in row.SelectNodes("td")
                        select new CellData { RowNum = rowcntr++, CellText = CleanUpWhiteSpace(cell.InnerText) };

                    candList.Add(FillCandidate(candTrRows.ToList(), candDesc));

                    //foreach (var cell in candTrRows)
                    //{
                    //    sb.AppendLine($"{cell.RowNum}, {cell.CellText}");
                    //}

                    sb.AppendLine("");
                }
            }
            foreach (var cand in candList)
            {
                sb.AppendLine(cand.ToCsv());
            }


            return sb;
        }


        public static CandidateSos FillCandidate(List<CellData> cellData, string officeName)
        {
            var candidate = new CandidateSos()
                { OfficeName = officeName};


            foreach (var data in cellData)
            {
                var cellTextLen = data.CellText.Length;

                switch (data.CellText.Substring(0, 5))
                {
                    case "E-mai":
                        candidate.Email = "Unavailable";
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
                            data.CellText = data.CellText.Remove(0, 14);
                        }
                        break;

                    case "WEBSI":
                        if (cellTextLen > 9)
                        {
                            candidate.Website = data.CellText.Remove(0, 9);
                        }

                        break;

                    default:

                        // Must be Name Address, CityStZip
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

                                break;
                        }

                        break;
                }

                
            }



            return candidate;
        }

        public static string CleanUpWhiteSpace(string desc)
        {
            return desc.Trim().Replace("\r\n", " ").Replace("\t", string.Empty);

        }

        public static StringBuilder TestAgility3(string html)
        {
            var theHtml = System.IO.File.ReadAllText(@"C:\Users\fred\Dropbox\VoterGuide\ScrapeSos\3_CandidateDetailsMar24.html");

            // declare object of HtmlDocument
            HtmlDocument doc = new HtmlDocument();
            
            doc.LoadHtml(theHtml);
            var rowcntr = 1;

            // Using LINQ to parse HTML table smartly 
            //var HTMLTableTRList = from table in doc.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
            //    from row in table.SelectNodes("//tr").Cast<HtmlNode>()
            //    from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
            //    select new { Table_Name = rowcntr++, Cell_Text = cell.InnerText };

            var HTMLTableTRList = from table in doc.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
                  select new { Table_Name = rowcntr++, Cell_Text = table.Attributes };

            // now showing output of parsed HTML table
            var sb = new StringBuilder();

            bool start = false;

            foreach (var table in HTMLTableTRList)
            {
                if (start)
                {
                    sb.AppendLine($"{table.Table_Name}, {table.Cell_Text}");
                }
                else
                {
                    start = table.Cell_Text.Contains("Qualified Candidates");
                }
            }

            //foreach (var cell in HTMLTableTRList)
            //{
            //    if (start)
            //    {
            //        sb.AppendLine($"{cell.Table_Name}, {cell.Cell_Text}");
            //    }
            //    else
            //    {
            //        start = cell.Cell_Text.Contains("Qualified Candidates");
            //    }
            //}

            return sb;
        }

        public static StringBuilder TestAgility2(string html)
        {
            // declare object of HtmlDocument
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(@"<table id=""TC""><tbody><tr><th>Name</th></tr><tr><td>Technology</td></tr><tr><td>Crowds</td></tr></tbody></table>");

            // Using LINQ to parse HTML table smartly 
            var HTMLTableTRList = from table in doc.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
                from row in table.SelectNodes("//tr").Cast<HtmlNode>()
                from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                select new { Table_Name = table.Id, Cell_Text = cell.InnerText };

            // now showing output of parsed HTML table
            var sb = new StringBuilder();
            
            foreach (var cell in HTMLTableTRList)
            {
                sb.AppendLine($"{cell.Table_Name}, {cell.Cell_Text}");
            }

            return sb;
        }
    }

    public class CellData
    {
        public int RowNum { get; set; }
        public string CellText { get; set; }
    }
}
/*
 *
    string a = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='a']").GetAttributeValue("value", "default");

 */
