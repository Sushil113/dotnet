namespace testing.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string Salt { get; set; } = String.Empty;    
    }
}
