using Microsoft.AspNetCore.Identity;

namespace QL_Spa.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}