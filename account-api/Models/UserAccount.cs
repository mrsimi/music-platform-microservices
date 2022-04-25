namespace account_api.Models
{
    public class UserAccount
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt {get; set;}
        public DateTime DateAdded {get; set;}
    }
}