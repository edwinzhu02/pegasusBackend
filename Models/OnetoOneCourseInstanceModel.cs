using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pegasus_backend.Models
{
    public class OnetoOneCourseInstanceModel
    {
        public int id { get; set; }
        public int? CourseId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public CourseScheduleModel Schedule { get; set; }
    }

    public class OnetoOneCourseInstancesModel
    {
        public List<OnetoOneCourseInstanceModel> OnetoOneCourses { get; set; }
    }
}