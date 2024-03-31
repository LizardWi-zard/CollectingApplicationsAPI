using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollectingApplicationsAPI.Model
{
    public class Application
    {
        [Column(TypeName = "uuid")]
        [Required]
        public Guid id { get; set; }
        
        [Required]
        [Column(TypeName = "uuid")]
        public Guid Author { get; set; }

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string? Activity { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string? Name { get; set; }
        
        [Column(TypeName = "varchar(300)")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string? Outline { get; set; }
        
        [Column(TypeName = "varchar")]
        public string? Status { get; set; }
        
        [Column(TypeName = "date")]
        public DateTime? EditTime { get; set; }
    }
}
