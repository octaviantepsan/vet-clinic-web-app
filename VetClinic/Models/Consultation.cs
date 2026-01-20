using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Consultation
    {
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Required]
        public string Diagnosis { get; set; } = string.Empty;
        
        public string? Treatment { get; set; }
        
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceCost { get; set; }

        public Bill? Bill { get; set; }
    }
}