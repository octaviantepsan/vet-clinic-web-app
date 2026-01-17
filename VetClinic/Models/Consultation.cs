using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Consultation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Diagnosis { get; set; } // Fixed: Must be provided

        public string? Treatment { get; set; } // Fixed: Made optional (maybe just a checkup?)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        // One-to-One relationship with Appointment
        [Required]
        public int AppointmentId { get; set; }
        
        [ForeignKey("AppointmentId")]
        public Appointment? Appointment { get; set; } // Fixed: Nullable to avoid constructor errors
    }
}