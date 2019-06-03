using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Models
{
    public class LoginLogModel
    {
        public int? LoginLogId { get; set; }
        [Required (ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        public int? LogType { get; set; }
        [Required (ErrorMessage = "Time is required")]
        public DateTime CreatedAt { get; set; }
        [Required (ErrorMessage = "OrgId is required")]
        public short OrgId { get; set; }

    }
}
