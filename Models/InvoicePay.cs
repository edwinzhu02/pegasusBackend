using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class InvoicePay
    {
        [Required(ErrorMessage = "Invoice is required")]
        public int InvoiceId { get; set; }
        [Required(ErrorMessage = "Invoice is required")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "Invoice is required")]
        public byte PaymentMethod { get; set; }
        [Required(ErrorMessage = "Learner is required")]
        public int LearnerId { get; set; }
        public short? StaffId { get; set; }
    }
}