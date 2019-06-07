using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pegasus_backend.pegasusContext;



namespace Pegasus_backend.Models
{
    public class LessonTerm
    {
        public int LessonId { set; get; }

        public int? LearnerId { set; get; }
        
        public short? IsCanceled { get; set; }
        
        public string Reason { get; set; }
        
        public int? CourseInstanceId { get; set; }
        
        public int? GroupCourseInstanceId { get; set; }
        
        public int? InvoiceId { get; set; }
        
        public byte? IsConfirm { get; set; }
        
        public short? TermId { get; set; }
       

    }
}