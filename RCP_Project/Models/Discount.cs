using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models
{
    public class Discount
    
    { 
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string AppliesTo { get; set; }
    }
}