using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;


namespace Pegasus_backend.Models
{
    public class LessonsViewModel
    {
        public int LessonId;
        public DateTime OriginalDate { get; set; }
        public short WeekNo { get; set; } = 1;
        public byte IsPaid { get; set; } = 0;
        public byte IsCompleted { get; set; } = 0;
        public short IsCanceled { get; set; } = 0;
        public byte IsMadeup { get; set; } = 0;
        public short Remaining { get; set; } = 0;
        public string MakeUpDetail { get; set; } ="";                  
    }
    public class LearnersLessonViewModel
    {
        public int LearnerId { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Course { get; set; } 
        public short DayOfWeek { get; set; } 
        public List<LessonsViewModel> LessonsViewModel {get ;set;}
    }
}