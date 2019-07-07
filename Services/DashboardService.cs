using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Services
{
    public class DashboardService
    {
        private readonly ablemusicContext _ablemusciContext;
        private readonly ILogger _log;
        private readonly List<int> _orgIds;
        private readonly DateTime _today;

        public DashboardService(ablemusicContext ablemusicContext, ILogger<Object> log, List<int> orgIds)
        {
            _ablemusciContext = ablemusicContext;
            _log = log;
            _orgIds = orgIds;
            _today = DateTime.UtcNow.ToNZTimezone();
        }

        public async Task<Result<List<Lesson>>> getLessonsForToday()
        {
            var result = new Result<List<Lesson>>();
            result.Data = new List<Lesson>();
            try
            {
                result.Data = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.BeginTime.Value.Date == _today.Date && 
                _orgIds.Contains(l.OrgId) && l.IsCanceled != 1).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Lesson>>> getTrialLessonsForToday()
        {
            var result = new Result<List<Lesson>>();
            result.Data = new List<Lesson>();
            try
            {
                result.Data = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.BeginTime.Value.Date == _today.Date && 
                l.IsTrial == 1 && _orgIds.Contains(l.OrgId) && l.IsCanceled != 1).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Learner>>> getNewStudentsForToday()
        {
            var result = new Result<List<Learner>>();
            result.Data = new List<Learner>();
            try
            {
                result.Data = await _ablemusciContext.Learner.Where(l => l.EnrollDate.HasValue && _today.Date == l.EnrollDate.Value.Date && 
                _orgIds.Contains((int)l.OrgId)).ToListAsync();
                Console.WriteLine(_today.Date);
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<InvoiceWaitingConfirm>>> getUnConfirmedWaitingInvoice()
        {
            var result = new Result<List<InvoiceWaitingConfirm>>();
            result.Data = new List<InvoiceWaitingConfirm>();
            try
            {
                result.Data = await (from w in _ablemusciContext.InvoiceWaitingConfirm
                                join l in _ablemusciContext.Learner on w.LearnerId equals l.LearnerId
                                where _orgIds.Contains((int)l.OrgId) && w.IsConfirmed == 0
                                select w
                                ).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Lesson>>> getCanceledLessonsForToday()
        {
            var result = new Result<List<Lesson>>();
            result.Data = new List<Lesson>();
            try
            {
                result.Data = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.IsCanceled == 1 && _orgIds.Contains(l.OrgId) && 
                l.BeginTime.Value.Date == _today.Date).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Lesson>>> getRearrangedLessonsForToday()
        {
            var result = new Result<List<Lesson>>();
            result.Data = new List<Lesson>();
            try
            {
                result.Data = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && _orgIds.Contains(l.OrgId) && l.IsCanceled != 1 && 
                l.IsChanged == 1 && l.BeginTime.Value.Date == _today.Date).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Amendment>>> getExpiredDayOffForToday()
        {
            var result = new Result<List<Amendment>>();
            result.Data = new List<Amendment>();
            try
            {
                result.Data = await _ablemusciContext.Amendment.Where(a => a.EndDate.HasValue && a.AmendType == 1 && 
                _today.Date == a.EndDate.Value.Date && _orgIds.Contains((int)a.OrgId)).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Amendment>>> getExpiredRearrangedLessonsForToday()
        {
            var result = new Result<List<Amendment>> ();
            result.Data = new List<Amendment>();
            try
            {
                result.Data = await _ablemusciContext.Amendment.Where(a => a.EndDate.HasValue && a.AmendType == 2 && 
                _today.Date == a.EndDate.Value.Date && _orgIds.Contains((int)a.OrgId)).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            return result;
        }

        public async Task<Result<List<Learner>>> getLearnerWithRemainLessonsForToday(int numOfRemainLesson)
        {
            var result = new Result<List<Learner>>();
            result.Data = new List<Learner>();
            var lessonsForToday = await getLessonsForToday();
            if (!lessonsForToday.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = lessonsForToday.ErrorMessage;
                return result;
            } 

            var learnerIdForToday = new List<int>();
            foreach(var lft in lessonsForToday.Data)
            {
                learnerIdForToday.Add(lft.LearnerId.Value);
            }

            var repeatedLearners = new List<Learner>();
            try
            {
                repeatedLearners = await (from l in _ablemusciContext.Lesson
                                join lr in _ablemusciContext.Learner on l.LearnerId equals lr.LearnerId
                                where learnerIdForToday.Contains(l.LearnerId.Value) && l.BeginTime.Value.Date >= _today.Date
                                select lr
                                ).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }

            while (repeatedLearners.Count > 0)
            {
                int currentLearnerId = repeatedLearners[0].LearnerId;
                int counter = 0;
                foreach(var rl in repeatedLearners)
                {
                    if (rl.LearnerId == currentLearnerId)
                    {
                        counter++;
                    }
                }
                
                if(counter == numOfRemainLesson + 1)
                {
                    result.Data.Add(repeatedLearners.Find(rl => rl.LearnerId == currentLearnerId));
                }
                repeatedLearners.RemoveAll(rl => rl.LearnerId == currentLearnerId);
            }

            return result;
        }

        public async Task<Result<Dictionary<DateTime, List<Lesson>>>> getRecentLessons(int numOfDays, DateTime calculateDate)
        {
            var result = new Result<Dictionary<DateTime, List<Lesson>>>();
            result.Data = new Dictionary<DateTime, List<Lesson>>();
            var daysBefore = numOfDays % 2 == 0 ? numOfDays / 2 : (numOfDays - 1) / 2;
            var daysLater = numOfDays % 2 == 0 ? (numOfDays / 2) - 1 : (numOfDays - 1) / 2;
            DateTime beginDate = calculateDate.AddDays(-daysBefore);
            DateTime endDate = calculateDate.AddDays(daysLater);
            var totalLessons = new List<Lesson>();
            try
            {
                totalLessons = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.IsCanceled != 1 && _orgIds.Contains(l.OrgId) &&
                l.BeginTime.Value.Date >= beginDate.Date && l.BeginTime.Value.Date <= endDate.Date).ToListAsync();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            
            var currentDate = beginDate;
            for(int i = 0; i < numOfDays; i++)
            {
                var eachDayLessons = new List<Lesson>();
                foreach (var tl in totalLessons)
                {
                    if (tl.BeginTime.Value.Date == currentDate.Date)
                    {
                        eachDayLessons.Add(tl);
                    }
                }
                result.Data.Add(currentDate, eachDayLessons);
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<Result<Dictionary<string, List<Learner>>>> getRecentNewRegisteredLearner(int numOfWeeks)
        {
            var result = new Result<Dictionary<string, List<Learner>>>();
            result.Data = new Dictionary<string, List<Learner>>();
            int totalDays = numOfWeeks * 7;
            DateTime beginTime = _today.AddDays(-totalDays);
            var totalLearners = new List<Learner>();
            try
            {
                totalLearners = await _ablemusciContext.Learner.Where(l => l.EnrollDate.HasValue && l.EnrollDate.Value.Date >= beginTime.Date && 
                l.EnrollDate.Value.Date <= _today.Date && _orgIds.Contains(l.OrgId.Value)).ToListAsync();
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                _log.LogError(ex.Message);
                return result;
            }
            var weekBeginDate = beginTime;
            for(int i = 0; i < numOfWeeks; i++)
            {
                string weekDuration = weekBeginDate.ToString("MM/dd/yyyy") + " - " + weekBeginDate.AddDays(6).ToString("MM/dd/yyyy");
                var eachWeekLearner = new List<Learner>();
                foreach(var tl in totalLearners)
                {
                    if(tl.EnrollDate.Value.Date >= weekBeginDate.Date && tl.EnrollDate.Value.Date <= weekBeginDate.AddDays(6).Date)
                    {
                        eachWeekLearner.Add(tl);
                    }
                }
                result.Data.Add(weekDuration, eachWeekLearner);
                weekBeginDate = weekBeginDate.AddDays(7);
            }

            return result;
        }
    }
}
