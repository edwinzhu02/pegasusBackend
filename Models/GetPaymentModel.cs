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
        public decimal? Other4Fee { get; set; }
        public string Other4FeeName { get; set; }    
        public decimal? Other5Fee { get; set; }
        public string Other5FeeName { get; set; }    
        public decimal? Other6Fee { get; set; }
        public string Other6FeeName { get; set; }    
        public decimal? Other7Fee { get; set; }
        public string Other7FeeName { get; set; }    
        public decimal? Other8Fee { get; set; }
        public string Other8FeeName { get; set; }    
        public decimal? Other9Fee { get; set; }
        public string Other9FeeName { get; set; }    
        public decimal? Other10Fee { get; set; }
        public string Other10FeeName { get; set; }    
        public decimal? Other11Fee { get; set; }
        public string Other11FeeName { get; set; }    
        public decimal? Other12Fee { get; set; }
        public string Other12FeeName { get; set; }    
        public decimal? Other13Fee { get; set; }
        public string Other13FeeName { get; set; }    
        public decimal? Other14Fee { get; set; }
        public string Other14FeeName { get; set; }    
        public decimal? Other15Fee { get; set; }
        public string Other15FeeName { get; set; }    
        public decimal? Other16Fee { get; set; }
        public string Other16FeeName { get; set; }    
        public decimal? Other17Fee { get; set; }
        public string Other17FeeName { get; set; }    
        public decimal? Other18Fee { get; set; }
        public string Other18FeeName { get; set; }          
    }

}