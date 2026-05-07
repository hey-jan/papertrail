using Microsoft.AspNetCore.Identity;

namespace PaperTrail.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Helper property combining FirstName and LastName
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
