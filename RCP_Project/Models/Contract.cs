using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models
{
    public class Contract
    {
        [Key]
        public int ID { get; set; }

        public int ClientID { get; set; }
        public virtual Client Client { get; set; }

        public int SoftwareID { get; set; }
        public virtual Software Software { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        [MaxLength(100)]
        public string Version { get; set; }

        public bool IsPaid { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}