using System;
using System.Collections.Generic;

namespace Pegasus_backend.pegasusContext
{
    public partial class News
    {
        public int NewsId { get; set; }
        public string NewsTitle { get; set; }
        public string NewsType { get; set; }
        public byte[] NewsData { get; set; }
        public byte Categroy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public short? IsTop { get; set; }
        public string TitleUrl { get; set; }
    }
}
