using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public enum AppointmentStatus
    {
        Pending = 0,
        Accepted = 1,
        Refused = 2,
        Completed = 3,         
        RescheduleProposed = 4 
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
        public Doctor? Doctor { get; set; }

        [Required]
        public int PetId { get; set; }
        public Pet? Pet { get; set; }

        public Consultation? Consultation { get; set; }

        public string? Description { get; set; }
    }
}