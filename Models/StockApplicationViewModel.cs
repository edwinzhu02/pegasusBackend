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
        public List<StockApplicationDetailsViewModel> ProductIdQty { get; set; }
    }
    public class StockApplicationDetailsViewModel
    {
       [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "AppliedQty is required")]
        public int? AppliedQty { get; set; }
    }

}
