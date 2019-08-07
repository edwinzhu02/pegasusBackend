using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pegasus_backend.Models
{
    public class CheckInOrOutModel
    {
        [Required (ErrorMessage = "UserId is required")]
        public short UserId { get; set; }
        [Required (ErrorMessage = "LocationX is required")]
        public decimal? LocaltionX { get; set; }
        [Required (ErrorMessage = "LocationY is required")]
        public decimal? LocaltionY { get; set; }
    }
}