using Pegasus_backend.pegasusContext;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class NoticeModel
    {
        // public int ProductId { get; set; }
        [Required(ErrorMessage = "Notice Content is Required.")]
        public string Notice { get; set; }
        [Required(ErrorMessage = "FromStaffId is Required.")]
        public short FromStaffId { get; set; }
        [Required(ErrorMessage = "ToStaffId is Required.")]
        public IEnumerable<short> ToStaffId { get; set; }
    }
}