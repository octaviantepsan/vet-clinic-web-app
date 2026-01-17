using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Pet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public required string Name { get; set; } // Fixed with 'required'

        [Required]
        [StringLength(50)]
        public required string Species { get; set; } // Fixed with 'required'

        public string? Breed { get; set; } // Fixed: Made optional (Nullable)

        public int Age { get; set; }
        public double Weight { get; set; }

        public string? ImageUrl { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        // Link to Owner (Client)
        [Required]
        public required string OwnerId { get; set; } // Fixed with 'required'

        [ForeignKey("OwnerId")]
        public ApplicationUser? Owner { get; set; } // Optional: EF Core handles this link automatically

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}