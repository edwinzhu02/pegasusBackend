using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Pegasus_backend.Models
{
    public class LearnerDayoffViewModel
    {
        [Required(ErrorMessage = "UserId is required")]
        public short UserId { get; set; }
        [Required(ErrorMessage = "LearnerId is required")]
        public int LearnerId { get; set; }
        [Required(ErrorMessage = "BeginDate is required")]
        public DateTime BeginDate { get; set; }
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
        [Required(ErrorMessage = "InstanceId is required")]
        public List<int> InstanceIds { get; set; }
    }
}
