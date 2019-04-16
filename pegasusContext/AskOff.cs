using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class AskOff
    {
        public int AskId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? ReasonType { get; set; }
        public string ReasonDesc { get; set; }
        public DateTime? ApplyDate { get; set; }
        public short? ProcessStatus { get; set; }
        public short? UserType { get; set; }
        public string Note { get; set; }
        public short? UserId { get; set; }

        public User User { get; set; }
    }
}
