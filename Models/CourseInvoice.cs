using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;

namespace Pegasus_backend.Models
{
    public class CourseInvoice
    {
        public Boolean IsGroupCourse { set; get; } = false;

        public int One2OneCourseInstanceId { set; get; }

        public One2oneCourseInstance One2oneCourseInstance {set;get;}

        public int GroupCourseInstanceId { set; get; }

        public GroupCourseInstance GroupCourseInstance { set; get; }

        public decimal? CoursePrice { set; get; }
        
        public IEnumerable<CourseSchedule> CourseSchedules { set; get; }

        public IEnumerable<Amendment> CourseAmendments { set; get; }

        public short? TeacherId { set; get; }

        public int? LeanerId { set; get; }

        public Course Course { set; get; }

        public DateTime? begin_date { set; get; }

        public DateTime? end_date { set; get; }
    }
}