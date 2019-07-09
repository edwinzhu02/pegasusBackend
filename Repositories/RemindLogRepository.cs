using Pegasus_backend.pegasusContext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pegasus_backend.Utilities;
using Pegasus_backend.Models;

namespace Pegasus_backend.Repositories
{
    public class RemindLogRepository
    {
        private readonly ablemusicContext _ablemusicContext;

        public RemindLogRepository()
        {
            _ablemusicContext = new ablemusicContext();
            _remindLogs = new List<RemindLog>();
        }

        public List<RemindLog> _remindLogs { get; }

        public void AddSingleRemindLog(int? learnerId, string email, string remindContent, short? teacherId, string remindTitle, int? lessonId)
        {
            _remindLogs.Add(new RemindLog
            {
                LearnerId = learnerId,
                Email = email,
                RemindType = 1,
                RemindContent = remindContent,
                CreatedAt = DateTime.UtcNow.ToNZTimezone(),
                TeacherId = teacherId,
                IsLearner = learnerId == null ? (short)0 : (short)1,
                ProcessFlag = 0,
                EmailAt = null,
                RemindTitle = remindTitle,
                ReceivedFlag = 0,
                LessonId = lessonId
            });
        }

        public void AddMultipleRemindLogs(Dictionary<Learner, string> learnerMapContent, short? teacherId, string remindTitle, int? lessonId)
        {
            foreach (KeyValuePair<Learner, string> lc in learnerMapContent)
            {
                AddSingleRemindLog(lc.Key.LearnerId, lc.Key.Email, lc.Value, teacherId, remindTitle, lessonId);
            }
        }

        public void AddMultipleRemindLogs(Dictionary<Teacher, string> teacherMapContent, short? learnerId, string remindTitle, int? lessonId)
        {
            foreach (KeyValuePair<Teacher, string> tc in teacherMapContent)
            {
                AddSingleRemindLog(learnerId, tc.Key.Email, tc.Value, tc.Key.TeacherId, remindTitle, lessonId);
            }
        }

        public async Task<Result<List<RemindLog>>> SaveRemindLogAsync()
        {
            var result = new Result<List<RemindLog>>();
            try
            {
                foreach(var r in _remindLogs)
                {
                    await _ablemusicContext.RemindLog.AddAsync(r);
                }
                await _ablemusicContext.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
            result.Data = _remindLogs;
            return result;
        }
    }
}
