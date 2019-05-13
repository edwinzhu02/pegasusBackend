using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class Holiday
    {
        public short HolidayId { get; set; }
        public DateTime HolidayDate { get; set; }
        public string HolidayName { get; set; }
    }
}
