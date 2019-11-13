using System;
using System.Linq;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using System.Collections.Generic;
using Pegasus_backend.Services;
using System.IO;
using Pegasus_backend.Utilities;



using Newtonsoft.Json;


namespace Pegasus_backend.Services
{
    public class InvoiceUpdateService : IInvoiceUpdateService
    {
        private const short firstInvoiceLessonQTY = 14; //if at the begining of a term,first invoice including lessons quantity
        private const short weekQTYBeforeNextTerm = 4;  //if remaining week quantity to define if at the begin of a term.
        private readonly ablemusicContext _ablemusicContext;
        public InvoiceUpdateService(ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }
        public bool InvoiceUpdate(List<int> lessonIds){
            foreach( var lessonId in lessonIds){
                findInvoice(lessonId);
            }
            _ablemusicContext.SaveChanges();
            return true;
        }
        private bool findInvoice(int lessonId){
            string invoiceNum = _ablemusicContext.Lesson.Where(i => i.LessonId ==lessonId)
                    .Select(i =>i.InvoiceNum).FirstOrDefault();
            if (invoiceNum == null) throw new Exception("Can not find this lessons!");
            InvoiceWaitingConfirm invoiceWaitingConfirm = _ablemusicContext.InvoiceWaitingConfirm
                        .Where(i => i.InvoiceNum == invoiceNum  &&i.IsActivate==1).FirstOrDefault();
            if (invoiceWaitingConfirm == null) throw new Exception("Can not find draft invoice!");
             if (updateInvoiceWaitingConfirm(ref invoiceWaitingConfirm))
                _ablemusicContext.Update(invoiceWaitingConfirm);
            else
                _ablemusicContext.Remove(invoiceWaitingConfirm);

            Invoice invoice = _ablemusicContext.Invoice
                        .Where(i => i.InvoiceNum == invoiceNum &&i.IsActive==1).FirstOrDefault();
            if (invoice != null){
                if (invoice.IsPaid == 1) throw new Exception("This lesson is paid ,can not be change!");
                if (updateInvoice(ref invoice))
                    _ablemusicContext.Update(invoice);
                else
                    _ablemusicContext.Remove(invoice);
            }
           
            return true;
        }
        private bool updateInvoiceWaitingConfirm(ref InvoiceWaitingConfirm invoice)
        {
            var unitPrice = invoice.LessonFee/invoice.LessonQuantity;
            invoice.LessonQuantity --;
            invoice.LessonFee -= unitPrice;
            invoice.TotalFee -= unitPrice;
            invoice.OwingFee -= unitPrice; 
            if (invoice.LessonQuantity==0) return false;
            return true;
        }
        private bool updateInvoice(ref Invoice invoice)
        {
            var unitPrice = invoice.LessonFee/invoice.LessonQuantity;
            invoice.LessonQuantity --;
            invoice.LessonFee -= unitPrice;
            invoice.TotalFee -= unitPrice;
            invoice.OwingFee -= unitPrice; 
            if (invoice.LessonQuantity==0) return false;            
            return true;
        }

    }
}