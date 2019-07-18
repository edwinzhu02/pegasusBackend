using System;
using System.Collections.Generic;
using Pegasus_backend.pegasusContext;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{

    public partial class ApplicationDetailsModel
    {
        [Required(ErrorMessage = "DetaillsId is required")]
        public int DetaillsId { get; set; }
        public int? DeliveredQty { get; set; }
        public int? ReceivedQty { get; set; }

    }
}