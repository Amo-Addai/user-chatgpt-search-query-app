using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ChatGPTBackend.Models
{

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int? Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; } // todo: remove (use PasswordHash instead)
        // public string? PasswordHash { get; set; }
    }

    public class Query
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        // Private field with initializer
        private string _userId = string.Empty;

        // Public property mapped to the private field
        public string UserId
        {
            get { return _userId; }
            set { _userId = value ?? string.Empty; } // Ensure that value is never null
        }
        
        public string? QueryText { get; set; }
        public string? ResponseText { get; set; }
    }
}
