using System;
using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models
{
    public class Payment
    {
        [Key]
        public int ID { get; set; }

        public int ContractID { get; set; }
        public virtual Contract Contract { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}