using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace Pegasus_backend.Models
{
    public class LessonsViewModel
    {
        public DateTime OriginalDate { get; set; }
        public short WeekNo { get; set; } = 1;
        public short IsPaid { get; set; } = 0;
        public short IsCompleted { get; set; } = 0;
        public short IsCanceled { get; set; } = 0;
        public short IsMadeup { get; set; } = 0;
        public short Remaining { get; set; } = 0;
        public string MakeUpDetail { get; set; } ="";                  
    }
    public class LearnersLessonViewModel
    {
        public int LearnerId { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Course { get; set; } 
        public List<LessonsViewModel> LessonsViewModel {get ;set;}
    }
}