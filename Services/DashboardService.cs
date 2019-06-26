using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pegasus_backend.pegasusContext;
using Microsoft.EntityFrameworkCore;


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
            try
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("New Zealand Standard Time");
                _today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nztZone);
            }
            catch (TimeZoneNotFoundException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                _today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nztZone);
            }
            catch (InvalidTimeZoneException)
            {
                TimeZoneInfo nztZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
                _today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, nztZone);
            }
        }

        public async Task<List<Lesson>> getLessonsForToday()
        {
            var result = new List<Lesson>();
            try
            {
                result = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.BeginTime.Value.Date == _today.Date && 
                _orgIds.Contains(l.OrgId) && l.IsCanceled != 1).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Lesson>> getTrialLessonsForToday()
        {
            var result = new List<Lesson>();
            try
            {
                result = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.BeginTime.Value.Date == _today.Date && 
                l.IsTrial == 1 && _orgIds.Contains(l.OrgId) && l.IsCanceled != 1).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Learner>> getNewStudentsForToday()
        {
            var result = new List<Learner>();
            try
            {
                result = await _ablemusciContext.Learner.Where(l => l.EnrollDate.HasValue && _today.Date == l.EnrollDate.Value.Date && 
                _orgIds.Contains((int)l.OrgId)).ToListAsync();
                Console.WriteLine(_today.Date);
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<InvoiceWaitingConfirm>> getUnConfirmedWaitingInvoice()
        {
            var result = new List<InvoiceWaitingConfirm>();
            try
            {
                result = await (from w in _ablemusciContext.InvoiceWaitingConfirm
                                join l in _ablemusciContext.Learner on w.LearnerId equals l.LearnerId
                                where _orgIds.Contains((int)l.OrgId) && w.IsConfirmed == 0
                                select w
                                ).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Lesson>> getCanceledLessonsForToday()
        {
            var result = new List<Lesson>();
            try
            {
                result = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.IsCanceled == 1 && _orgIds.Contains(l.OrgId) && 
                l.BeginTime.Value.Date == _today.Date).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Lesson>> getRearrangedLessonsForToday()
        {
            var result = new List<Lesson>();
            try
            {
                result = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && _orgIds.Contains(l.OrgId) && l.IsCanceled != 1 && 
                l.IsChanged == 1 && l.BeginTime.Value.Date == _today.Date).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Amendment>> getExpiredDayOffForToday()
        {
            var result = new List<Amendment>();
            try
            {
                result = await _ablemusciContext.Amendment.Where(a => a.EndDate.HasValue && a.AmendType == 1 && 
                _today.Date == a.EndDate.Value.Date && _orgIds.Contains((int)a.OrgId)).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Amendment>> getExpiredRearrangedLessonsForToday()
        {
            var result = new List<Amendment> ();
            try
            {
                result = await _ablemusciContext.Amendment.Where(a => a.EndDate.HasValue && a.AmendType == 2 && 
                _today.Date == a.EndDate.Value.Date && _orgIds.Contains((int)a.OrgId)).ToListAsync();
            }
            catch(Exception ex)
            {
                _log.LogError(ex.Message);
            }
            return result;
        }

        public async Task<List<Learner>> getLearnerWithRemainLessonsForToday(int numOfRemainLesson)
        {
            var lessonsForToday = await getLessonsForToday();
            var learnerIdForToday = new List<int>();
            foreach(var lft in lessonsForToday)
            {
                learnerIdForToday.Add(lft.LearnerId.Value);
            }

            var result = new List<Learner>();
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
                _log.LogError(ex.Message);
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
                    result.Add(repeatedLearners.Find(rl => rl.LearnerId == currentLearnerId));
                }
                repeatedLearners.RemoveAll(rl => rl.LearnerId == currentLearnerId);
            }

            return result;
        }

        public async Task<Dictionary<DateTime, List<Lesson>>> getRecentLessons(int numOfDays)
        {
            var result = new Dictionary<DateTime, List<Lesson>>();
            var daysBefore = numOfDays % 2 == 0 ? numOfDays / 2 : (numOfDays - 1) / 2;
            var daysLater = numOfDays % 2 == 0 ? (numOfDays / 2) - 1 : (numOfDays - 1) / 2;
            DateTime beginDate = _today.AddDays(-daysBefore);
            DateTime endDate = _today.AddDays(daysLater);
            var totalLessons = new List<Lesson>();
            try
            {
                totalLessons = await _ablemusciContext.Lesson.Where(l => l.BeginTime.HasValue && l.IsCanceled != 1 && _orgIds.Contains(l.OrgId) &&
                l.BeginTime.Value.Date >= beginDate.Date && l.BeginTime.Value.Date <= endDate.Date).ToListAsync();
            }
            catch (Exception ex)
            {
                _log.LogError(ex.Message);
            }
            
            var currentDate = beginDate;
            for(int i = 0; i < numOfDays; i++)
            {
                var eachDayLessons = new List<Lesson>();
                foreach (var tl in totalLessons)
                {
                    if (tl.BeginTime.Value.Date == currentDate)
                    {
                        eachDayLessons.Add(tl);
                    }
                }
                result.Add(currentDate, eachDayLessons);
                currentDate = currentDate.AddDays(1);
            }

            return result;
        }

        public async Task<Dictionary<string, List<Learner>>> getRecentNewRegisteredLearner(int numOfWeeks)
        {
            var result = new Dictionary<string, List<Learner>>();
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
                _log.LogError(ex.Message);
            }
            var weekBeginDate = beginTime;
            for(int i = 0; i < numOfWeeks; i++)
            {
                string weekDuration = weekBeginDate.ToString() + " - " + weekBeginDate.AddDays(6).ToString();
                var eachWeekLearner = new List<Learner>();
                foreach(var tl in totalLearners)
                {
                    if(tl.EnrollDate >= weekBeginDate && tl.EnrollDate <= weekBeginDate.AddDays(6))
                    {
                        eachWeekLearner.Add(tl);
                    }
                }
                result.Add(weekDuration, eachWeekLearner);
                weekBeginDate = weekBeginDate.AddDays(7);
            }

            return result;
        }
    }
}
