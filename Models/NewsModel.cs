
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class NewsModel
    {
        public string NewsTitle { get; set; }
        public string NewsType { get; set; }
        public string newsData { get; set; }
        public byte Categroy { get; set; }
        public int UserId { get; set; }
        public short? IsTop { get; set; }
        public string TitleUrl { get; set; }
    }
}