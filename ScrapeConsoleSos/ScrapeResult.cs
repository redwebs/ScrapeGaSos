using System.Collections.Generic;
using PageScrapeSos;

namespace ScrapeConsoleSos
{
    // The background worker returns this object to the UI

    public class ScrapeResult
    {
        public ScrapeOp Operation { get; set; } = ScrapeOp.Elections;

        public bool ErrorEncountered { get; set; } = false;

        public string ErrorMessage { get; set; } = string.Empty;

        public int CandidatesScraped { get; set; } = 0;

        public List<Candidate> Candidates { get; set; }

        public List<Election> Elections { get; set; }

        public string ElapsedTime { get; set; } = string.Empty;

        public SequenceStatus SequenceStat { get; set; }

        public bool ScrapeSuccess { get; set; } = false;

        public ScrapeStatus ScrapeStat { get; set; }

    }

    public enum ScrapeOp
    {
        Elections = 1,

        Candidates  = 2
    }
}
