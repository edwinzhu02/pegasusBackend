using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class InvoicePay
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public byte PaymentMethod { get; set; }
        [Required(ErrorMessage = "Learner is required")]
        public int LearnerId { get; set; }
        public short? StaffId { get; set; }
    }
}