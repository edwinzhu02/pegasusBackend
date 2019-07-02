using System.Collections.Generic;
using System;
using System.Linq;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.ActionFilter;
using Pegasus_backend.Models;
using Pegasus_backend.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
namespace Pegasus_backend.Models
{
    public class CourseRemain
    {
        public int LessonRemainId { get; set; }
        public int? Quantity { get; set; }
        public short? TermId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public int? LearnerId { get; set; }
        public string CourseName { get; set; }
        
        public int? UnconfirmLessons { get; set; }
        
        public Term Term { get; set; }
    }
}