using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Consultation
    {
        public int Id { get; set; }

        // Link to the Appointment (One-to-One)
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        // Medical Info (Doctor fills this)
        [Required]
        public string Diagnosis { get; set; } = string.Empty;
        
        public string? Treatment { get; set; } // Meds, surgery details
        
        public string? Notes { get; set; } // Private doctor notes

        // Financial Info (Doctor enters cost, system creates bill)
        [Column(TypeName = "decimal(18,2)")]
        public decimal ServiceCost { get; set; }

        // Link to the Bill (Automatically created)
        public Bill? Bill { get; set; }
    }
}