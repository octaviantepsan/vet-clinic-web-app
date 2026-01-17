using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Accepted = 1,
        Refused = 2,
        Completed = 3,         // For when the visit is done
        RescheduleProposed = 4 // NEW: Admin suggests a new time
    }

    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DateTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; } // Make nullable to avoid constructor warnings

        [Required]
        public int PetId { get; set; }
        public Pet? Pet { get; set; } // Make nullable to avoid constructor warnings

        public Consultation? Consultation { get; set; }

        public string? Description { get; set; }
    }
}