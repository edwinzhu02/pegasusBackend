using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class InvoicePay
    {
        [Required(ErrorMessage = "Invoice is required")]
        public int InvoiceId { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "PaymentMethod is required")]
        public byte PaymentMethod { get; set; }
        [Required(ErrorMessage = "LearnerId is required")]
        public int LearnerId { get; set; }
        [Required(ErrorMessage = "staffId is required")]
        public short? StaffId { get; set; }
    }
}