using System.Collections.Generic;

namespace Pegasus_backend.Models
{
    public class CheckInOrOutModel
    {
        public short UserId { get; set; }
        public decimal? LocaltionX { get; set; }
        public decimal? LocaltionY { get; set; }
    }
}