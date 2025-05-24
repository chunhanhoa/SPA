namespace LoanSpa.Models
{
    public class AppUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Trong thực tế, lưu hash của password
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; } // 'Admin', 'User', ...
    }
}
