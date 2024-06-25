using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RCP_Project.Models
{
    public abstract class Client
    {
        [Key]
        public int ID { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string PhoneNumber { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Contract> Contracts { get; set; }
    }

    public class Individual : Client
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string PESEL { get; set; }
    }

    public class Company : Client
    {
        [MaxLength(100)]
        public string CompanyName { get; set; }

        [MaxLength(100)]
        public string KRS { get; set; }
    }
}