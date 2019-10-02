﻿using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class StockApplication
    {
        public StockApplication()
        {
            ApplicationDetails = new HashSet<ApplicationDetails>();
        }

        public short? OrgId { get; set; }
        public short? ApplyStaffId { get; set; }
        public DateTime? ApplyAt { get; set; }
        public int ApplicationId { get; set; }
        public byte? ProcessStatus { get; set; }
        public string ApplyReason { get; set; }
        public string ReplyContent { get; set; }
        public short? IsDisputed { get; set; }
        public DateTime? ReplyAt { get; set; }
        public DateTime? RecieveAt { get; set; }
        public DateTime? DeliverAt { get; set; }

        public virtual Staff ApplyStaff { get; set; }
        public virtual Org Org { get; set; }
        public virtual ICollection<ApplicationDetails> ApplicationDetails { get; set; }
    }
}
