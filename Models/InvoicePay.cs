namespace Pegasus_backend.Models
{
    public class InvoicePay
    {
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public int LearnerId { get; set; }
        public short? StaffId { get; set; }
    }
}