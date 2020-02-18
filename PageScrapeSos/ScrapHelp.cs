using System;
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
            var theHtml =
                System.IO.File.ReadAllText(@"C:\Users\fred\Dropbox\VoterGuide\ScrapeSos\3_CandidateDetailsMar24.html");

            // declare object of HtmlDocument
            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(theHtml);

            const string tgtDiv = "//*[@class=\"col1Inner\"]/table/tr";
            var nodes = htmlDoc.DocumentNode.SelectNodes(tgtDiv);

            if (nodes == null)
            {
                //CurrentStatus.ScrapeComplete = true;
                //CurrentStatus.LastOpMessage = "Data table search returned null.";
                //return false;
            }

            var candDesc = string.Empty;
            var candTdCnt = 1;

            foreach (var nodetr in nodes)
            {
                var tdObj = nodetr.ChildNodes[1];  // td

                if (tdObj.Attributes[0].Name == "colspan")
                {
                    // Start of candidate section
                    candDesc = tdObj.InnerText.Trim();
                    candTdCnt = 0;
                }
                else
                {
                    // Get the two TD's that have Tables of candidate info.
                    var tableObj = tdObj.ChildNodes[1];

                    for (var idx = 1; idx < tableObj.ChildNodes.Count + 1; idx++)
                    {
                        if (tableObj.ChildNodes.Count > 0)
                        {
                            var tr = tableObj.ChildNodes[1];

                            if (tr.ChildNodes.Count > 0)
                            {
                                var td = tr.ChildNodes[1];

                                var txt = td.InnerText;
                            }
                        }
                    }


                    if (candTdCnt == 1)
                    {
                        
                    }

                }

            }

            return new StringBuilder();
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
}
/*
 *
    string a = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='a']").GetAttributeValue("value", "default");

 */
