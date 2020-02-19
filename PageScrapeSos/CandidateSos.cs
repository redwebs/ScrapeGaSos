using System;
using System.Text;

namespace PageScrapeSos
{
    public class CandidateSos
    {
        private const string QualifyingCandidateUrl = "https://elections.sos.ga.gov/GAElection/CandidateDetails";

        public string ElectionId { get; set; }

        public int Year { get; set; }

        public string OfficeName { get; set; } = string.Empty;

        public string CandidateName { get; set; }

        public string Address { get; set; } = string.Empty;

        public string CityStZip { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Party { get; set; } = string.Empty;

        public string Occupation { get; set; } = string.Empty;

        public bool Incumbent { get; set; } = false;

        public string QualifiedDate { get; set; } = string.Empty;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("CandidateName: ");
            sb.Append(CandidateName);
            sb.Append(", Address: ");
            sb.Append(Address);
            sb.Append(", CityStZip: ");
            sb.Append(CityStZip);
            sb.Append(", Email: ");
            sb.Append(Email);

            sb.Append(", Party: ");
            sb.Append(Party);
            sb.Append(", PhoneNumber: ");
            sb.Append(PhoneNumber);
            sb.Append(", Occupation: ");
            sb.Append(Occupation);

            sb.Append(", OfficeName: ");
            sb.Append(OfficeName);
            sb.Append(", QualifiedDate: ");
            sb.Append(QualifiedDate);

            sb.Append(", Incumbent: ");
            sb.Append(Incumbent ? "Yes" : "No");
            sb.Append(", Year: ");
            sb.Append(Year);

            return sb.ToString();
        }

        public string CsvHeader()
        {
            var sb = new StringBuilder();

            sb.Append("\"CandidateName\",");
            sb.Append("\"Address\",");
            sb.Append("\"CityStZip\",");
            sb.Append("\"Email\",");

            sb.Append("\"Party\",");
            sb.Append("\"PhoneNumber\",");
            sb.Append("\"Occupation\",");

            sb.Append("\"OfficeName\",");
            sb.Append("\"QualifiedDate\",");

            sb.Append("\"Incumbent\",");
            sb.Append("\"Year\"");

            return sb.ToString();
        }

        public string ToCsv()
        {
            var sb = new StringBuilder();
            sb.Append($"\"{CandidateName}\",");
            sb.Append($"\"{Address}\",");
            sb.Append($"\"{CityStZip}\",");
            sb.Append($"\"{Email}\",");
            sb.Append($"\"{Party}\",");
            sb.Append($"\"{PhoneNumber}\",");
            sb.Append($"\"{Occupation}\",");
            sb.Append($"\"{CityStZip}\",");
            sb.Append($"\"{OfficeName}\",");
            sb.Append($"\"{QualifiedDate}\",");
            sb.Append($"\"{Incumbent}\",");
            sb.Append($"\"{Year}\"");
            return sb.ToString();
        }
    }
}
