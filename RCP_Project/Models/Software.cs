using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models
{
    public class Software
    {
        [Key]
        public int ID { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(10000)]
        public string Description { get; set; }
        [MaxLength(100)]
        public string CurrentVersion { get; set; }
        public decimal UpfrontCost { get; set; }
        [MaxLength(100)]
        public string Category { get; set; }
    }
}