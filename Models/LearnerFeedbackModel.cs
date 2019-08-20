namespace Pegasus_backend.Models
{
    public class LearnerFeedbackModel
    {
        public short UserId { get; set; }
        public int LessonId { get; set; }
        public short RateStar { get; set; }
        public string CommentToTeacher { get; set; }
    }
}