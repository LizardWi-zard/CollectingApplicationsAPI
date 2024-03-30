using System.Net;

namespace CollectingApplicationsAPI
{
    public class DbResponse
    {
        public HttpStatusCode Status { get; set; }

        public object? Data { get; set; }
    }
}
