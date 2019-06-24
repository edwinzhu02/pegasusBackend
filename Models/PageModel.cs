using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pegasus_backend.Models
{
    public class PageModel
    {

        [Required(ErrorMessage = "PageName is required")]
        public string PageName { get; set; }
        public short? PageGroupId { get; set; }
        public short? DisplayOrder { get; set; }
        public string Para { get; set; }
        public short? ParaFlag { get; set; }
        [Required(ErrorMessage = "Url is required")]
        public string Url { get; set; }
        public string Icon { get; set; }

    }
}
