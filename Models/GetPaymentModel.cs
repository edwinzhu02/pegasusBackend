using System;
using System.Collections.Generic;

namespace Pegasus_backend.Models
{
    public class GetPaymentModel
    {
        public int PaymentId { get; set; }
        public byte? PaymentMethod { get; set; }
        public int? LearnerId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? StaffId { get; set; }
        public int? InvoiceId { get; set; }
        public decimal? BeforeBalance { get; set; }
        public decimal? AfterBalance { get; set; }
        public byte? PaymentType { get; set; }
        public short? IsConfirmed { get; set; }
        public string Comment { get; set; }

         public PaymentInvoiceModel Invoice { get; set; }
         public PaymentLearnerModel Learner { get; set; }
         public PaymentStaffModel Staff { get; set; }
         public List<PaymentSoldTransaction> SoldTransaction { get; set; }
     }
         public class PaymentStaffModel
    {
     
        public string FirstName { get; set; }
        public string Visa { get; set; }
     
        public string LastName { get; set; }

    }
        public class PaymentSoldTransaction
    {
        public int? TranId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int ProductId { get; set; }
        public decimal? Balance { get; set; }
        public int SoldQuantity { get; set; }
        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountRate { get; set; }
        public decimal? DiscountedAmount { get; set; }
        public PaymentProductModel Product { get; set; }

    }

    public class PaymentProductModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }

    }
    public class PaymentLearnerModel
    {
        public int LearnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class PaymentInvoiceModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNum { get; set; }
        public decimal? LessonFee { get; set; }
        public decimal? ConcertFee { get; set; }
        public decimal? NoteFee { get; set; }
        public decimal? Other1Fee { get; set; }
        public decimal? Other2Fee { get; set; }
        public decimal? Other3Fee { get; set; }
        public int? LearnerId { get; set; }
        public string LearnerName { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? TotalFee { get; set; }
        public short? TermId { get; set; }
        public int? LessonQuantity { get; set; }
        public string CourseName { get; set; }
        public string ConcertFeeName { get; set; }
        public string LessonNoteFeeName { get; set; }
        public string Other1FeeName { get; set; }
        public string Other2FeeName { get; set; }
        public string Other3FeeName { get; set; }
    }

}