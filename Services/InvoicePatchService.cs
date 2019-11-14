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
    public class InvoicePatchService : IInvoicePatchService
    {
        private const short firstInvoiceLessonQTY = 14; //if at the begining of a term,first invoice including lessons quantity
        private const short weekQTYBeforeNextTerm = 4;  //if remaining week quantity to define if at the begin of a term.
        private readonly ablemusicContext _ablemusicContext;
        public InvoicePatchService(ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
        }
        public bool InvoicePatch(List<int> courseInstanceIds)
        {
            return true;
            foreach (var courseInstanceId in courseInstanceIds)
            {
                List<InvoiceWaitingConfirm> invoices = new List<InvoiceWaitingConfirm>();
                if (!findInvoice(courseInstanceId, ref invoices)) return false;
                if (!tranInvoice(invoices)) return false;
            }
            return true;
        }
        private bool findInvoice(int courseInstanceId, ref List<InvoiceWaitingConfirm> invoices)
        {

            invoices = _ablemusicContext.InvoiceWaitingConfirm
                .Where(i => i.CourseInstanceId == courseInstanceId ).OrderBy(i => i.TermId).ToList();
            if (invoices.Count < 2) return false;

            return true;
        }
        private bool isAtTermBegining(short? termId)
        {
            DateTime dateNow = DateTime.UtcNow.ToNZTimezone();

            var term = _ablemusicContext.Term
                .Where(i => i.TermId == termId).FirstOrDefault();

            TimeSpan ts = term.EndDate.Value.Subtract(dateNow);

            int totalWeeks = (int)(ts.Days / 7);

            if (totalWeeks > weekQTYBeforeNextTerm) return true;
            return false;
        }
        /*if today is at the begin 8 weeks of this term , the first invoice is 12 lessons ,the second invoice is remaining lessons*/
        /*if today is at the back 4 weeks of this term , combine two invoice to one invoice*/
        private bool tranInvoice(List<InvoiceWaitingConfirm> invoices)
        {
            short? termId = invoices[0].TermId;
            short? nextTermId = invoices[1].TermId;

            var lessons = _ablemusicContext.Lesson.Where(l => l.InvoiceNum == invoices[0].InvoiceNum).AsEnumerable();
            var nextLessons = _ablemusicContext.Lesson.Where(l => l.InvoiceNum == invoices[1].InvoiceNum).AsEnumerable();
            Decimal? unitPrice = invoices[0].LessonFee / invoices[0].LessonQuantity;
            if (lessons == null && nextLessons == null)
                return false;
            int lessonCounter = lessons.Count();
            Boolean isAtTermBegin = isAtTermBegining(termId);
            DateTime invoiceDate = new DateTime();
            foreach (var lesson in nextLessons)
            {
                lesson.InvoiceNum = invoices[0].InvoiceNum;
                invoices[0].LessonQuantity = invoices[0].LessonQuantity + 1;
                invoices[0].LessonFee = invoices[0].LessonFee + unitPrice;
                invoices[0].TotalFee = invoices[0].TotalFee + unitPrice;
                invoices[0].OwingFee = invoices[0].OwingFee + unitPrice;

                invoices[1].LessonQuantity = invoices[1].LessonQuantity - 1;
                invoices[1].LessonFee = invoices[1].LessonFee - unitPrice;
                invoices[1].TotalFee = invoices[1].TotalFee - unitPrice;
                invoices[1].OwingFee = invoices[1].OwingFee - unitPrice;
                _ablemusicContext.Update(lesson);
                invoiceDate = lesson.BeginTime.Value.Date;
                if (isAtTermBegin)
                    if (++lessonCounter >= firstInvoiceLessonQTY) break;
            }
                invoices[0].EndDate = invoiceDate;
                invoices[1].BeginDate = invoiceDate.AddDays(1);
                _ablemusicContext.Update(invoices[0]);
                _ablemusicContext.Update(invoices[1]);

            if (invoices[1].LessonQuantity==0)
                _ablemusicContext.Remove(invoices[1]);
            return true;
        }
    }
}