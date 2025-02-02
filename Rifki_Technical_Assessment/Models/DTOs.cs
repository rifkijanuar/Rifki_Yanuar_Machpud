namespace Rifki_Technical_Assessment.Models
{
    public class DTOs
    {
        public record LoginRequest(string Username, string Password);
        public record RegisterRequest(string Username, string Email, string Password);

        public record ProductRequest(string Name, string Description, decimal Price);

    }
}
