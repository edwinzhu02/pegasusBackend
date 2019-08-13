namespace Pegasus_backend.Models
{
    public class TeacherFeedbackModel
    {
        public short UserId { get; set; }
        public int LessonId { get; set; }
        public short RateStar { get; set; }
        public string CommentToLearner { get; set; }
        public string CommentToSchool { get; set; }
    }
}