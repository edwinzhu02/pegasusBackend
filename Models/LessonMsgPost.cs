using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class LessonMsgPost
    {
        [Required(ErrorMessage = "Id  is Required.")]
        public int Id { get; set; }
        [Required(ErrorMessage = "LessonId  is Required.")]
        public int LessonId { get; set; }        
        [Required(ErrorMessage = "role  is Required.")]        
        public short role { get; set; }
        [Required(ErrorMessage = "MessageContent  is Required.")]        
        public string MessageContent { get; set; }
           
    }
}