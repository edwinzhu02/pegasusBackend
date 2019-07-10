using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class StockApplicationDeliverAndReceiveViewModel
    {
        [Required(ErrorMessage = "ApplicationId is required")]
        public int? ApplicationId { get; set; }
        [Required(ErrorMessage = "ApplicationDetailsIdMapQty is required")]
        public Dictionary<int, int> ApplicationDetailsIdMapQty { get; set; }
    }
}
