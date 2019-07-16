using System;
using System.Collections.Generic;
using Pegasus_backend.pegasusContext;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
   public class  StockApplicationModel1
    {
        [Required(ErrorMessage = "ReplyAt is required")]
        public DateTime? ReplyAt { get; set; }
        [Required(ErrorMessage = "ReplyContent is required")]
        public string ReplyContent { get; set; }


    } 
     public class StockApplicationModel2
   {
        [Required(ErrorMessage = "DeliverAt is required")]
        public DateTime? DeliverAt { get; set; }
        public List<ApplicationDetailsModel> ApplicationDetailsModel { get; set; }

   } 
     public class StockApplicationModel3
   {
        [Required(ErrorMessage = "ReceivedQty is required")]
        public DateTime? RecieveAt { get; set; }

        public List<ApplicationDetailsModel> ApplicationDetailsModel { get; set; }


   } 
}
