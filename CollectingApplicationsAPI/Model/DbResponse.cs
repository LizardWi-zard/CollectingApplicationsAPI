using System.Net;

namespace CollectingApplicationsAPI.Model
{
    public class DbResponse
    {
        public HttpStatusCode Status { get; set; }

        public object? Data { get; set; }
    }
}
