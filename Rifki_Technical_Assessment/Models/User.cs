namespace Rifki_Technical_Assessment.Models
{
    public class User : BaseModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime? ExpiredToken { get; set; }
    }
}
