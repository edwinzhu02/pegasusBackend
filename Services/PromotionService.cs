using System;
using System.Linq;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using System.Collections.Generic;
using System.IO;



using Newtonsoft.Json;


namespace Pegasus_backend.Services
{
    public class PromotionService:IPromotionService
    {
        private readonly ablemusicContext _ablemusicContext;  
        public PromotionService(ablemusicContext ablemusicContext)
        {
            _ablemusicContext = ablemusicContext;
       }              
        public bool PromotionInvoice(ref InvoiceWaitingConfirm invoice)
        {
            List<PromotionInvoice> promotionObj= new  List<PromotionInvoice>();
            try{
                short promotionId = GetPromotionId(invoice.GroupCourseInstanceId);
                int whichTerm = WhichTerm(invoice.GroupCourseInstanceId,invoice.LearnerId);
                if (promotionId == 0) return false; //no promotion
                if (GetPromotionExp(promotionId,out promotionObj)==false) return false;
                parseExp(whichTerm,promotionObj,ref invoice);
            }
            catch (Exception e){
                return false;
            }
            return true;
        }
        private short GetPromotionId(int? groupCourseInstanceId)
        {
            var groupCourseInstance = _ablemusicContext.GroupCourseInstance.
            FirstOrDefault(x => x.GroupCourseInstanceId == groupCourseInstanceId);
            if (groupCourseInstance==null) return 0;
            if (groupCourseInstance.PromotionId==null) return 0;
            return groupCourseInstance.PromotionId.Value;
        }
        private int WhichTerm(int? groupCourseInstanceId,int? learnerId)
        {
            int invoiceQTY = _ablemusicContext.InvoiceWaitingConfirm.
                Where(x => x.GroupCourseInstanceId == groupCourseInstanceId
                && x.LearnerId==learnerId && x.IsActivate ==1 ).Count();
            // if (groupCourseInstance==null) return 0;
            // if (groupCourseInstance.PromotionId==null) return 0;
            return invoiceQTY;
        }
        private bool parseExp(int whichTerm,List<PromotionInvoice> promotionObj,ref InvoiceWaitingConfirm invoice)
        {
            PromotionInvoice termInvoice = promotionObj[whichTerm];
            decimal feeAmt = termInvoice.Amt;
            SetInvoiceInitFee(ref invoice,feeAmt);
            int idx = 0;
            if (termInvoice.Item==null) return true;
            foreach (var item in termInvoice.Item){
                SetInvoiceItem(ref invoice,item.Name,item.Amount,idx++);
            }
            return true;
        }
        private void SetInvoiceItem(ref InvoiceWaitingConfirm invoice,string name,decimal? feeAmt,int index)
        {
            switch (index)
            {
                // case 0:
                //     invoice.LessonFee=feeAmt;
                //     invoice.CourseName=name;
                //     break;
                case 0:
                    invoice.ConcertFee=feeAmt;
                    invoice.ConcertFeeName=name;
                    break;
                case 1:
                    invoice.NoteFee=feeAmt;
                    invoice.LessonNoteFeeName=name;
                    break;
                case 2:
                    invoice.NoteFee=feeAmt;
                    invoice.LessonNoteFeeName=name;
                    break;
                case 3:
                    invoice.Other1Fee=feeAmt;
                    invoice.Other1FeeName=name;
                    break;
                case 4:
                    invoice.Other2Fee=feeAmt;
                    invoice.Other2FeeName=name;
                    break;
                case 5:
                    invoice.Other3Fee=feeAmt;
                    invoice.Other3FeeName=name;
                    break;
                default:
                    break;
            }
        }
        private void SetInvoiceInitFee(ref InvoiceWaitingConfirm invoice,decimal feeAmt)
        {
            invoice.TotalFee=feeAmt;
            invoice.OwingFee=feeAmt;

            invoice.LessonFee=feeAmt;

            invoice.ConcertFee=null;
            invoice.ConcertFeeName=null;
            invoice.NoteFee=null;
            invoice.LessonNoteFeeName=null;

            invoice.Other1Fee=null;
            invoice.Other1FeeName=null;
            invoice.Other2Fee=null;
            invoice.Other2FeeName=null;
            invoice.Other3Fee=null;
            invoice.Other3FeeName=null;
        }
               
        private bool GetPromotionExp(short promotionId,out List<PromotionInvoice> PromotionExp)
        {
            var Promotion = _ablemusicContext.Promotion.
            FirstOrDefault(x => x.PromotionId == promotionId);
            PromotionExp = null;
            if (Promotion==null) return false;
            // List<Lesson> lessonsTobeAppend = new List<Lesson>();
            // var tempInvoice = new List<PromotionInvoice>();
            PromotionExp =JsonConvert.DeserializeObject<List<PromotionInvoice>>(Promotion.PromotionExp);
           
            return true;
        }
    }
}