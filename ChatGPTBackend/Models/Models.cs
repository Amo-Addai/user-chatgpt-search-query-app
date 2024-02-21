using Microsoft.EntityFrameworkCore;

namespace ChatGPTBackend.Models
{

    public class User
    {
        public int? Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        // Add other user properties as needed
    }

    public class Query
    {
        public int? Id { get; set; }
        public string? UserId { get; set; }
        public string? QueryText { get; set; }
        public string? ResponseText { get; set; }
        // Add other query properties as needed
    }
}
