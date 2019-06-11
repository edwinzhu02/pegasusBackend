using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class TeacherWageRates
    {
        public short TeacherId { get; set; }
        public decimal? PianoRates { get; set; }
        public decimal? OthersRates { get; set; }
        public decimal? GroupRates { get; set; }
        public decimal? TheoryRates { get; set; }
        public int RatesId { get; set; }
        public DateTime? CreateAt { get; set; }
        public short? IsActivate { get; set; }

        public Teacher Teacher { get; set; }
    }
}
