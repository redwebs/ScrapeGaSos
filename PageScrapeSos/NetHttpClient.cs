using System.Net.Http;

namespace PageScrapeSos
{
    public static class NetHttpClient
    {
        private static readonly HttpClient TheHttpClient = new HttpClient();

        static NetHttpClient() { }

        public static HttpClient Client => TheHttpClient;
    }
}
