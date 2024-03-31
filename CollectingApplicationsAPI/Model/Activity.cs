using Microsoft.EntityFrameworkCore;

namespace CollectingApplicationsAPI.Model
{
    public class Activity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}
