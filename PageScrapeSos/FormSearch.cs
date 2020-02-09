using System.Collections.Generic;
using System.Text;

namespace PageScrapeSos
{

    public class FormDataVar
    {
        public FormDataVar()
        {
        }

        public FormDataVar(string varName)
        {
            VarName = varName;
        }

        public FormDataVar(string varName, string varValue)
        {
            VarName = varName;
            VarValue = varValue;
        }

        public string VarName { get; set; }
        public string VarValue { get; set; }

        public override string ToString()
        {
            return $"{VarName}=${VarValue}";
        }
    }
    
    public class FormSearchSos
    {
        /*

	    1. nbElecYear:2020
	    2. id_election:35214
	    3. cd_party:
	    4. cdOfficeType:
	    5. id_office:0
	    6. cdFlow:S

        */

        public FormDataVar ElectionYear { get; set; } = new FormDataVar("nbElecYear");
        public FormDataVar ElectionId { get; set; } = new FormDataVar("id_election");
        public FormDataVar Party { get; set; } = new FormDataVar("cd_party");
        public FormDataVar OfficeType { get; set; } = new FormDataVar("cdOfficeType");
        public FormDataVar OfficeId { get; set; } = new FormDataVar("id_office");
        public FormDataVar Flow { get; set; } = new FormDataVar("cdFlow");

        public FormSearchSos()
        {
        }

        public FormSearchSos(string year)
        {
            ElectionYear.VarValue = year;
            ElectionId.VarValue = string.Empty;
            Party.VarValue = string.Empty;
            OfficeType.VarValue = string.Empty;
            OfficeId.VarValue = "0";
            Flow.VarValue = "S";
        }

        public FormSearchSos(string year, string electionId)
        {
            ElectionYear.VarValue = year;
            ElectionId.VarValue = electionId;

            Party.VarValue = string.Empty;
            OfficeType.VarValue = string.Empty;
            OfficeId.VarValue = "0";
            Flow.VarValue = "S";
        }

        public List<KeyValuePair<string, string>> FormDataList ()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(ElectionYear.VarName, ElectionYear.VarValue),
                new KeyValuePair<string, string>(ElectionId.VarName, ElectionId.VarValue),
                new KeyValuePair<string, string>(Party.VarName, Party.VarValue),
                new KeyValuePair<string, string>(OfficeType.VarName, OfficeType.VarValue),
                new KeyValuePair<string, string>(OfficeId.VarName, OfficeId.VarValue),
                new KeyValuePair<string, string>(Flow.VarName, Flow.VarValue),
            };
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.AppendLine(ElectionYear.ToString());
            sb.Append("Party = ");
            sb.AppendLine(Party.ToString());
            sb.Append("OfficeType   = ");
            sb.AppendLine(OfficeType.ToString());
            sb.Append("OfficeId     = ");
            sb.AppendLine(OfficeId.ToString());
            sb.Append("Flow");
            sb.AppendLine(Flow.ToString());
            sb.AppendLine("--");

            return sb.ToString();
        }

        public string ToSingleLine()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.Append(ElectionYear);
            sb.Append("Party = ");
            sb.Append(Party);
            sb.Append("OfficeType   = ");
            sb.Append(OfficeType);
            sb.Append("OfficeId     = ");
            sb.Append(OfficeId);
            sb.Append("Flow");
            sb.Append(Flow);
            sb.AppendLine("--");

            return sb.ToString();
        }
    }

    
    
    
    
    
    
    
    
    public class FormSearch
    {
        public string ElectionYear { get; set; } = string.Empty;
        public string OfficeTypeId { get; set; } = string.Empty;
        public string OfficeName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Circuit { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string County { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FilerId { get; set; } = string.Empty;

        public FormSearch()
        {
        }

        public FormSearch(string year, int officeTypeId, string officeName)
        {
            ElectionYear = year;
            OfficeName = officeName;
            OfficeTypeId = officeTypeId.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.AppendLine(ElectionYear);
            sb.Append("OfficeTypeId = ");
            sb.AppendLine(OfficeTypeId);
            sb.Append("OfficeName   = ");
            sb.AppendLine(OfficeName);
            sb.Append("District     = ");
            sb.AppendLine(District);
            sb.Append("Circuit      = ");
            sb.AppendLine(Circuit);
            sb.Append("Division     = ");
            sb.AppendLine(Division);
            sb.Append("County       = ");
            sb.AppendLine(County);
            sb.Append("City         = ");
            sb.AppendLine(City);
            sb.Append("FilerId      = ");
            sb.AppendLine(FilerId);
            sb.AppendLine("--");

            return sb.ToString();
        }

        public string ToSingleLine()
        {
            var sb = new StringBuilder();
            sb.Append("ElectionYear = ");
            sb.Append(ElectionYear);
            sb.Append(", OfficeTypeId = ");
            sb.Append(OfficeTypeId);
            sb.Append(", OfficeName = ");
            sb.Append(OfficeName.Replace("%20", " "));
            sb.Append(", District = ");
            sb.Append(District);
            sb.Append(", Circuit = ");
            sb.Append(Circuit);
            sb.Append(", Division = ");
            sb.Append(Division);
            sb.Append(", County = ");
            sb.Append(County);
            sb.Append(", City = ");
            sb.Append(City);
            sb.Append(", FilerId = ");
            sb.AppendLine(FilerId);

            return sb.ToString();
        }
    }
}
