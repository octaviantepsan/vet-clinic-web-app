using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Models
{
    public class Bill
    {
        public int Id { get; set; }

        public int ConsultationId { get; set; }
        public Consultation? Consultation { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public bool IsPaid { get; set; } = false;
        
        public DateTime? PaymentDate { get; set; }
    }
}