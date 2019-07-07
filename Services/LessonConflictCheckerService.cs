using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Utilities;


namespace Pegasus_backend.Services
{
    public class LessonConflictCheckerService
    {
        private readonly ablemusicContext _ablemusicContext;
        private readonly int _roomId;
        private readonly int _orgId;
        private readonly int _lessonId;
        private readonly DateTime _beginTime;
        private readonly DateTime _endTime;
        private readonly int _teacherId;
        
        public LessonConflictCheckerService(DateTime beginTime, DateTime endTime, int roomId = 0, int orgId = 0, int teacherId = 0, int lessonId = 0)
        {
            _ablemusicContext = new ablemusicContext();
            _roomId = roomId;
            _orgId = orgId;
            _lessonId = lessonId;
            _beginTime = beginTime;
            _endTime = endTime;
            _teacherId = teacherId;
        }

        public LessonConflictCheckerService(Lesson lesson)
        {
            _ablemusicContext = new ablemusicContext();
            _roomId = lesson.RoomId ?? 0;
            _orgId = lesson.OrgId ;
            _lessonId = lesson.LessonId;
            _beginTime = lesson.BeginTime ?? throw new ArgumentNullException(nameof(_beginTime));
            _endTime = lesson.EndTime ?? throw new ArgumentNullException(nameof(_endTime));
            _teacherId = lesson.TeacherId ?? 0;
        }

        public async Task<Result<List<Lesson>>> CheckRoomConflictInScheduledLessons()
        {
            var result = new Result<List<Lesson>>();
            var conflictRooms = new List<Lesson>();
            conflictRooms = await _ablemusicContext.Lesson.Where(l => l.RoomId == _roomId &&
                    l.OrgId == _orgId && l.IsCanceled != 1 && l.LessonId != _lessonId &&
                    ((l.BeginTime > _beginTime && l.BeginTime < _endTime) ||
                    (l.EndTime > _beginTime && l.EndTime < _endTime) ||
                    (l.BeginTime <= _beginTime && l.EndTime >= _endTime) ||
                    (l.BeginTime > _beginTime && l.EndTime < _endTime)))
                    .ToListAsync();

            if (conflictRooms.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room is not available by checking current scheduled lessons";
                result.Data = conflictRooms;
                return result;
            }
            result.Note = "No conflict room was found based on current scheduled lessons";
            return result;
        }

        public async Task<Result<List<Lesson>>> CheckTeacherConflictInScheduledLessons()
        {
            var result = new Result<List<Lesson>>();
            var conflictTeachersWithoutRelocation = new List<Lesson>();
            DateTime beginTime = _beginTime.AddMinutes(-60);
            DateTime endTime = _endTime.AddMinutes(60);
            conflictTeachersWithoutRelocation = await _ablemusicContext.Lesson.Where(l => l.TeacherId == _teacherId &&
                    l.IsCanceled != 1 && l.LessonId != _lessonId &&
                    ((l.BeginTime > beginTime && l.BeginTime < endTime) ||
                    (l.EndTime > beginTime && l.EndTime < endTime) ||
                    (l.BeginTime <= beginTime && l.EndTime >= endTime)))
                    .ToListAsync();

            if (conflictTeachersWithoutRelocation.Count > 0)
            {
                var conflictTeachers = new List<Lesson>();
                foreach (var c in conflictTeachersWithoutRelocation)
                {
                    if (c.OrgId != _orgId ||
                        (c.BeginTime > _beginTime && c.BeginTime < _endTime) ||
                        (c.EndTime > _beginTime && c.EndTime < _endTime) ||
                        (c.BeginTime <= _beginTime && c.EndTime >= _endTime))
                    {
                        conflictTeachers.Add(c);
                    }
                }
                result.IsSuccess = false;
                result.ErrorMessage = "Teacher is not available by checking current scheduled lessons";
                result.Data = conflictTeachers;
                return result;
            }
            result.Note = "No conflict teacher was found based on current scheduled lessons";
            return result;
        }

        public async Task<Result<List<object>>> CheckRoomConflictInUnscheduledLessons()
        {
            var result = new Result<List<object>>();
            result.Data = new List<object>();
            TimeSpan beginTime = _beginTime.TimeOfDay;
            TimeSpan endTime = _endTime.TimeOfDay;
            if(_beginTime.Date != _endTime.Date)
            {
                throw new Exception("The lesson begin time and end time are not at same date");
            }
            dynamic conflictRoomForOto = await (from cs in _ablemusicContext.CourseSchedule
                                                join oto in _ablemusicContext.One2oneCourseInstance on cs.CourseInstanceId equals oto.CourseInstanceId
                                                join c in _ablemusicContext.Course on oto.CourseId equals c.CourseId
                                                join o in _ablemusicContext.Org on oto.OrgId equals o.OrgId
                                                join r in _ablemusicContext.Room on oto.RoomId equals r.RoomId
                                                join t in _ablemusicContext.Teacher on oto.TeacherId equals t.TeacherId
                                                where oto.RoomId == _roomId && oto.OrgId == _orgId && _beginTime.Date >= oto.BeginDate.Value.Date &&
                                                (oto.EndDate.Value == null || (oto.EndDate.HasValue && _endTime.Date <= oto.EndDate.Value.Date)) && 
                                                _beginTime.ToDayOfWeek() == cs.DayOfWeek && 
                                                ((cs.BeginTime > beginTime && cs.BeginTime < endTime) ||
                                                (cs.EndTime > beginTime && cs.EndTime < endTime) ||
                                                (cs.BeginTime <= beginTime && cs.EndTime >= endTime) ||
                                                (cs.BeginTime > beginTime && cs.EndTime < endTime))
                                            select new
                                            {
                                                CourseScheduleId = cs.CourseScheduleId,
                                                CourseInstanceId = cs.CourseInstanceId,
                                                DayOfWeek = cs.DayOfWeek,
                                                BeginTime = cs.BeginTime,
                                                EndTime = cs.EndTime,
                                                CourseId = oto.CourseId,
                                                TeacherId = oto.TeacherId,
                                                OrgId = oto.OrgId,
                                                RoomId = oto.RoomId,
                                                CourseName = c.CourseName,
                                                OrgName = o.OrgName,
                                                RoomName = r.RoomName,
                                                TeacherFirstName = t.FirstName,
                                                TeacherLastName = t.LastName
                                            }).ToListAsync();

            dynamic conflictRoomForGroupCourse = await (from cs in _ablemusicContext.CourseSchedule
                                                        join gc in _ablemusicContext.GroupCourseInstance on cs.GroupCourseInstanceId equals gc.GroupCourseInstanceId
                                                        join c in _ablemusicContext.Course on gc.CourseId equals c.CourseId
                                                        join o in _ablemusicContext.Org on gc.OrgId equals o.OrgId
                                                        join r in _ablemusicContext.Room on gc.RoomId equals r.RoomId
                                                        join t in _ablemusicContext.Teacher on gc.TeacherId equals t.TeacherId
                                                        where gc.RoomId == _roomId && gc.OrgId == _orgId && _beginTime.Date >= gc.BeginDate.Value.Date &&
                                                        (gc.EndDate.Value == null || (gc.EndDate.HasValue && _endTime.Date <= gc.EndDate.Value.Date)) &&
                                                        _beginTime.ToDayOfWeek() == cs.DayOfWeek &&
                                                        ((cs.BeginTime > beginTime && cs.BeginTime < endTime) ||
                                                        (cs.EndTime > beginTime && cs.EndTime < endTime) ||
                                                        (cs.BeginTime <= beginTime && cs.EndTime >= endTime) ||
                                                        (cs.BeginTime > beginTime && cs.EndTime < endTime))
                                                        select new
                                                        {
                                                            CourseScheduleId = cs.CourseScheduleId,
                                                            CourseInstanceId = cs.CourseInstanceId,
                                                            DayOfWeek = cs.DayOfWeek,
                                                            BeginTime = cs.BeginTime,
                                                            EndTime = cs.EndTime,
                                                            CourseId = gc.CourseId,
                                                            TeacherId = gc.TeacherId,
                                                            OrgId = gc.OrgId,
                                                            RoomId = gc.RoomId,
                                                            CourseName = c.CourseName,
                                                            OrgName = o.OrgName,
                                                            RoomName = r.RoomName,
                                                            TeacherFirstName = t.FirstName,
                                                            TeacherLastName = t.LastName
                                                        }).ToListAsync();

            foreach (var conflictRoomOTO in conflictRoomForOto)
            {
                result.Data.Add(conflictRoomOTO);
            }
            foreach(var conflictRoomGC in conflictRoomForGroupCourse)
            {
                result.Data.Add(conflictRoomGC);
            }
            if(result.Data.Count > 0)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Room is not available by checking unscheduled lessons";
                return result;
            }

            result.Note = "No conflict room was found based on unscheduled lessons";
            return result;
        }

        public async Task<Result<List<object>>> CheckTeacherConflictInUnscheduledLessons()
        {
            var result = new Result<List<object>>
            {
                Data = new List<object>()
            };
            TimeSpan beginTime = _beginTime.AddMinutes(-60).TimeOfDay;
            TimeSpan endTime = _endTime.AddMinutes(60).TimeOfDay;
            if (_beginTime.Date != _endTime.Date)
            {
                throw new Exception("The lesson begin time and end time are not at same date");
            }
            dynamic conflictTeacherWithOutRelacationForOto = await (from cs in _ablemusicContext.CourseSchedule
                                                                    join oto in _ablemusicContext.One2oneCourseInstance on cs.CourseInstanceId equals oto.CourseInstanceId
                                                                    join c in _ablemusicContext.Course on oto.CourseId equals c.CourseId
                                                                    join o in _ablemusicContext.Org on oto.OrgId equals o.OrgId
                                                                    join r in _ablemusicContext.Room on oto.RoomId equals r.RoomId
                                                                    join t in _ablemusicContext.Teacher on oto.TeacherId equals t.TeacherId
                                                                    where oto.TeacherId == _teacherId && _beginTime.Date >= oto.BeginDate.Value.Date &&
                                                                    (oto.EndDate.Value == null || (oto.EndDate.HasValue && _endTime.Date <= oto.EndDate.Value.Date)) &&
                                                                    _beginTime.ToDayOfWeek() == cs.DayOfWeek &&
                                                                    ((cs.BeginTime > beginTime && cs.BeginTime < endTime) ||
                                                                    (cs.EndTime > beginTime && cs.EndTime < endTime) ||
                                                                    (cs.BeginTime <= beginTime && cs.EndTime >= endTime))
                                                                    select new
                                                                    {
                                                                        CourseScheduleId = cs.CourseScheduleId,
                                                                        CourseInstanceId = cs.CourseInstanceId,
                                                                        DayOfWeek = cs.DayOfWeek,
                                                                        BeginTime = cs.BeginTime,
                                                                        EndTime = cs.EndTime,
                                                                        CourseId = oto.CourseId,
                                                                        TeacherId = oto.TeacherId,
                                                                        OrgId = oto.OrgId,
                                                                        RoomId = oto.RoomId,
                                                                        CourseName = c.CourseName,
                                                                        OrgName = o.OrgName,
                                                                        RoomName = r.RoomName,
                                                                        TeacherFirstName = t.FirstName,
                                                                        TeacherLastName = t.LastName
                                                                    }).ToListAsync();
            dynamic conflictTeacherWithOutRelacationForGroupCourse = await (from cs in _ablemusicContext.CourseSchedule
                                                                            join gc in _ablemusicContext.GroupCourseInstance on cs.GroupCourseInstanceId equals gc.GroupCourseInstanceId
                                                                            join c in _ablemusicContext.Course on gc.CourseId equals c.CourseId
                                                                            join o in _ablemusicContext.Org on gc.OrgId equals o.OrgId
                                                                            join r in _ablemusicContext.Room on gc.RoomId equals r.RoomId
                                                                            join t in _ablemusicContext.Teacher on gc.TeacherId equals t.TeacherId
                                                                            where gc.TeacherId == _teacherId && _beginTime.Date >= gc.BeginDate.Value.Date &&
                                                                            (gc.EndDate.Value == null || (gc.EndDate.HasValue && _endTime.Date <= gc.EndDate.Value.Date)) &&
                                                                            _beginTime.ToDayOfWeek() == cs.DayOfWeek &&
                                                                            ((cs.BeginTime > beginTime && cs.BeginTime < endTime) ||
                                                                            (cs.EndTime > beginTime && cs.EndTime < endTime) ||
                                                                            (cs.BeginTime <= beginTime && cs.EndTime >= endTime))
                                                                            select new
                                                                            {
                                                                                CourseScheduleId = cs.CourseScheduleId,
                                                                                CourseInstanceId = cs.CourseInstanceId,
                                                                                DayOfWeek = cs.DayOfWeek,
                                                                                BeginTime = cs.BeginTime,
                                                                                EndTime = cs.EndTime,
                                                                                CourseId = gc.CourseId,
                                                                                TeacherId = gc.TeacherId,
                                                                                OrgId = gc.OrgId,
                                                                                RoomId = gc.RoomId,
                                                                                CourseName = c.CourseName,
                                                                                OrgName = o.OrgName,
                                                                                RoomName = r.RoomName,
                                                                                TeacherFirstName = t.FirstName,
                                                                                TeacherLastName = t.LastName
                                                                            }).ToListAsync();
            var conflictTeachers = new List<object>();
            foreach(var conflictOTO in conflictTeacherWithOutRelacationForOto)
            {
                if(conflictOTO.OrgId != _orgId || 
                    (conflictOTO.BeginTime > _beginTime.TimeOfDay && conflictOTO.BeginTime < _endTime.TimeOfDay) ||
                    (conflictOTO.EndTime > _beginTime.TimeOfDay && conflictOTO.EndTime < _endTime.TimeOfDay) ||
                    (conflictOTO.BeginTime <= _beginTime.TimeOfDay && conflictOTO.EndTime >= _endTime.TimeOfDay))
                {
                    conflictTeachers.Add(conflictOTO);
                }
            }
            foreach(var conflicGroupCourse in conflictTeacherWithOutRelacationForGroupCourse)
            {
                if (conflicGroupCourse.OrgId != _orgId ||
                    (conflicGroupCourse.BeginTime > _beginTime.TimeOfDay && conflicGroupCourse.BeginTime < _endTime.TimeOfDay) ||
                    (conflicGroupCourse.EndTime > _beginTime.TimeOfDay && conflicGroupCourse.EndTime < _endTime.TimeOfDay) ||
                    (conflicGroupCourse.BeginTime <= _beginTime.TimeOfDay && conflicGroupCourse.EndTime >= _endTime.TimeOfDay))
                {
                    conflictTeachers.Add(conflicGroupCourse);
                }
            }

            if(conflictTeachers != null)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Teacher is not available by checking unscheduled lessons";
                result.Data = conflictTeachers;
                return result;
            }

            result.Note = "No conflict teacher was found based on unscheduled lessons";
            return result;
        }

        public async Task<Result<List<object>>> CheckBothRoomAndTeacher()
        {
            var result = new Result<List<object>>();
            var checkRoomInScheduledLessonsResult = await CheckRoomConflictInScheduledLessons();
            var checkTeacherInScheduledLessonsResult = await CheckTeacherConflictInScheduledLessons();
            var checkRoomInUnscheduledLessonsResult = await CheckRoomConflictInUnscheduledLessons();
            var checkTeacherInUnscheduledLessonsResult = await CheckTeacherConflictInUnscheduledLessons();

            if(!checkRoomInScheduledLessonsResult.IsSuccess || !checkTeacherInScheduledLessonsResult.IsSuccess || !checkRoomInUnscheduledLessonsResult.IsSuccess)
            {
                result.IsSuccess = false;
                result.ErrorMessage = "Details are in inner result";
                result.Data = new List<object>
                {
                    checkRoomInScheduledLessonsResult,
                    checkTeacherInScheduledLessonsResult,
                    checkRoomInUnscheduledLessonsResult,
                    checkTeacherInUnscheduledLessonsResult
                };
                return result;
            }
            result.Note = "There is no any conflict";
            return result;
        }
    }
}
