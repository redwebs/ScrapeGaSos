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
            var theHtml = System.IO.File.ReadAllText(@"C:\Users\fred\Dropbox\VoterGuide\ScrapeSos\SosMar24_CandidateDetails.html");

            // declare object of HtmlDocument
            HtmlDocument doc = new HtmlDocument();
            
            doc.LoadHtml(theHtml);

            // Using LINQ to parse HTML table smartly 
            var HTMLTableTRList = from table in doc.DocumentNode.SelectNodes("//table").Cast<HtmlNode>()
                from row in table.SelectNodes("//tr").Cast<HtmlNode>()
                from cell in row.SelectNodes("th|td").Cast<HtmlNode>()
                select new { Table_Name = table.Id, Cell_Text = cell.InnerText };

            // now showing output of parsed HTML table
            var sb = new StringBuilder();

            bool start = false;

            foreach (var cell in HTMLTableTRList)
            {
                if (start)
                {
                    sb.AppendLine($"{cell.Table_Name}, {cell.Cell_Text}");
                }
                else
                {
                    start = cell.Cell_Text.Contains("Qualified Candidates");
                }
            }

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
