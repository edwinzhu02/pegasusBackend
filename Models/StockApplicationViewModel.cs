using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class StockApplicationViewModel
    {
        [Required(ErrorMessage = "OrgId is required")]
        public short? OrgId { get; set; }
        [Required(ErrorMessage = "ApplyStaffId is required")]
        public short? ApplyStaffId { get; set; }
        [Required(ErrorMessage = "ApplyReason is required")]
        public string ApplyReason { get; set; }
        [Required(ErrorMessage = "ProductIdMapQty is required")]
        public Dictionary<int, int> ProductIdMapQty { get; set; }
    }
}
