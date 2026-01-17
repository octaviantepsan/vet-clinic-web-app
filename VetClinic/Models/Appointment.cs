using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public enum AppointmentStatus { Pending, Accepted, Refused, Completed, Cancelled }

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
    }
}