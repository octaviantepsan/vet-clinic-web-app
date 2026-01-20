using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public bool IsDarkMode { get; set; } = false;
        public string Language { get; set; } = "en";

        public string FullName => $"{FirstName} {LastName}";
    }
}