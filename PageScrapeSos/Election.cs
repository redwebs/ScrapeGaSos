
namespace PageScrapeSos
{
    public class Election
    {
        public Election()
        {
        }

        public Election(string electionId, string electionName)
        {
            ElectionId = electionId;
            ElectionName = electionName;
        }

        public string ElectionId { get; set; }  // id_election select name on page

        public string ElectionName { get; set; }
    }
}
