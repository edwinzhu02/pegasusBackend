using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class Mail
    {
        [Required(ErrorMessage = "MailTo  is Required.")]
        public string MailTo { get; set; }
        [Required(ErrorMessage = "MailTitle  is Required.")]        
        public string MailTitle { get; set; }
        [Required(ErrorMessage = "MailContent  is Required.")]        
        public string MailContent { get; set; }
        public byte[] Attachment { get; set; }
           
    }
}