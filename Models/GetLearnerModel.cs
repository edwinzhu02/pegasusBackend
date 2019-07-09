using System;
using System.Collections.Generic;

namespace Pegasus_backend.Models
{
    public class GetLearnerModel
    {
        public int LearnerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? EnrollDate { get; set; }
        public string ContactNum { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public short? IsUnder18 { get; set; }
        public DateTime? Dob { get; set; }
        public short? Gender { get; set; }
        public short? IsAbrsmG5 { get; set; }
        public string G5Certification { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ReferrerLearnerId { get; set; }
        public string Photo { get; set; }
        public string Note { get; set; }
        public string MiddleName { get; set; }
        public byte? LevelType { get; set; }
        public short? UserId { get; set; }
        public short? OrgId { get; set; }
        public short? IsActive { get; set; }
        public byte? LearnerLevel { get; set; }
        public byte? PaymentPeriod { get; set; }
        public string Referrer { get; set; }
        public string Comment { get; set; }
        public string FormUrl { get; set; }
        public string OtherfileUrl { get; set; }
        public List<GetLearnerParentModel> Parent { get; set; }
        public List<GetLearnerLearnerOthers> LearnerOthers { get; set; }
        public List<GetLearnerOnetoOneCourseInstance> One2oneCourseInstance { get; set; }
        public List<GetLearnerLearnerGroupCourse> LearnerGroupCourse { get; set; }
    }

    public class GetLearnerLearnerGroupCourse
    {
        public int LearnerGroupCourseId { get; set; }
        public int? LearnerId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short? IsActivate { get; set; }
        public string Comment { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public GetLearnerLearnerGroupCourseGroupCourseInstance GroupCourseInstance { get; set; }
    }

    public class GetLearnerLearnerGroupCourseGroupCourseInstance
    {
        public int CourseId { get; set; }
        public short? TeacherId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? RoomId { get; set; }
        public short? OrgId { get; set; }
        public int GroupCourseInstanceId { get; set; }
        public short? IsActivate { get; set; }
        public short? IsStarted { get; set; }
        public GetLearnerLearnerGroupCourseGroupCourseInstanceTeacher Teacher { get; set; }
        public GetLearnerLearnerGroupCourseGroupCourseInstanceCourse Course { get; set; }
        public GetLearnerLearnerGroupCourseGroupCourseInstanceRoom Room { get; set; }
        public List<GetLearnerLearnerGroupCourseGroupCourseInstanceCourseSchedule> CourseSchedule { get; set; }
    }

    public class GetLearnerLearnerGroupCourseGroupCourseInstanceRoom
    {
        public short RoomId { get; set; }
        public short? OrgId { get; set; }
        public string RoomName { get; set; }
        public short? IsActivate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class GetLearnerLearnerGroupCourseGroupCourseInstanceCourse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public byte? CourseType { get; set; }
        public byte? Level { get; set; }
        public short? Duration { get; set; }
        public decimal? Price { get; set; }
        public short? CourseCategoryId { get; set; }
        public byte? TeacherLevel { get; set; }
    }
    
    public class GetLearnerLearnerGroupCourseGroupCourseInstanceCourseSchedule
    {
        public int CourseScheduleId { get; set; }
        public byte? DayOfWeek { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
    

    public class GetLearnerLearnerGroupCourseGroupCourseInstanceTeacher
    {
        public short TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    

    
    
    public class GetLearnerParentModel
    {
        public int ParentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNum { get; set; }
        public byte? Relationship { get; set; }
        public int? LearnerId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class GetLearnerLearnerOthers
    {
        public int LearnerId { get; set; }
        public string OthersType { get; set; }
        public short? OthersValue { get; set; }
        public int LearnerOthersId { get; set; }
        public short? LearnerLevel { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstance
    {
        public int CourseInstanceId { get; set; }
        public int? CourseId { get; set; }
        public short? TeacherId { get; set; }
        public short? OrgId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public GetLearnerOnetoOneCourseInstanceOrg Org { get; set; }
        public GetLearnerOnetoOneCourseInstanceCourse Course { get; set; }
        public GetLearnerOnetoOneCourseInstanceRoom Room { get; set; }
        public GetLearnerOnetoOneCourseInstanceCourseTeacher Teacher { get; set; }
        public List<GetLearnerOnetoOneCourseInstanceCourseAmendment> Amendment { get; set; }
        public List<GetLearnerOnetoOneCourseInstanceCourseSchedule> CourseSchedule { get; set; }
        
    }
    
    public class GetLearnerOnetoOneCourseInstanceCourseAmendment
    {
        public int? CourseInstanceId { get; set; }
        public int AmendmentId { get; set; }
        public short? OrgId { get; set; }
        public short? DayOfWeek { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? LearnerId { get; set; }
        public short? RoomId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Reason { get; set; }
        public short? IsTemporary { get; set; }
        public byte? AmendType { get; set; }
        public int? CourseScheduleId { get; set; }
        public short? TeacherId { get; set; }
        public GetLearnerOnetoOneCourseInstanceCourseAmendmentTeacher Teacher { get; set; }
        public GetLearnerOnetoOneCourseInstanceCourseAmendmentRoom Room { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceCourseAmendmentRoom
    {
        public short RoomId { get; set; }
        public short? OrgId { get; set; }
        public string RoomName { get; set; }
        public short? IsActivate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceCourseAmendmentTeacher
    {
        public short TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceCourseTeacher
    {
        public short TeacherId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class GetLearnerOnetoOneCourseInstanceCourseSchedule
    {
        public int CourseScheduleId { get; set; }
        public byte? DayOfWeek { get; set; }
        public int? CourseInstanceId { get; set; }
        public int? GroupCourseInstanceId { get; set; }
        public TimeSpan? BeginTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceOrg
    {
        public short OrgId { get; set; }
        public string OrgName { get; set; }
        public short? IsHeadoffice { get; set; }
        public short? IsActivate { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal? LocaltionX { get; set; }
        public decimal? LocaltionY { get; set; }
        public string Abbr { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceCourse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public byte? CourseType { get; set; }
        public byte? Level { get; set; }
        public short? Duration { get; set; }
        public decimal? Price { get; set; }
        public short? CourseCategoryId { get; set; }
        public byte? TeacherLevel { get; set; }
    }

    public class GetLearnerOnetoOneCourseInstanceRoom
    {
        public short RoomId { get; set; }
        public short? OrgId { get; set; }
        public string RoomName { get; set; }
        public short? IsActivate { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}