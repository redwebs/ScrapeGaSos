using System;
using System.Collections.Generic;
using System.Text;

namespace PageScrapeSos
{
    public class ScrapeStatus
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ScrapeStatus()
        {
            SbLog = new StringBuilder();
        }

        public string Url { get; set; }

        public Uri TheUri { get; set; }

        public bool ScrapeComplete { get; set; } = false;

        public int TotalPages { get; set; }

        public int TotalCandidates { get; set; }

        public int LastPageCompleted { get; set; }

        private string _message = string.Empty;

        public string LastOpMessage
        {
            get => _message;

            set

            {
                _message = value;
                if (InternalLoggingOn)
                {
                    SbLog.AppendLine(_message);
                }
                if (LoggingOn)
                {
                    Log.Debug(_message);
                }
            }
        }
        
        public StringBuilder SbLog { get; }

        public bool InternalLoggingOn { private get; set; } = true;

        public bool LoggingOn { get; set; } = true;

        public List<Election> Elections { get; set; }


        public override string ToString()
        {
            return SbLog.ToString();
        }
    }
}
