using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using Microsoft.Extensions.Logging;

namespace Pegasus_backend.Controllers.MobileControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : BasicController
    {
        private readonly IMapper _mapper;


        public ChatController(ablemusicContext ablemusicContext, ILogger<NavItemsController> log) : base(ablemusicContext, log)
        {
        }

        //GET: http://localhost:5000/api/Chat/GetChatListOfTeacher/:userId
        // Get chat list of teacher
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetChatListOfTeacher(int id)
        {
            Result<IEnumerable<Lesson>> result = new Result<IEnumerable<Lesson>>();
            var teacherDetail = await _ablemusicContext.Teacher.Where(x => x.UserId == id).FirstOrDefaultAsync();
            if (teacherDetail == null)
            {
                return NotFound("No such a teacher");
            }
            // find available courses of teacher
            var lessionsOfTeacher = await _ablemusicContext.Lesson
                .Where(x => x.TeacherId == teacherDetail.TeacherId && x.IsCanceled == 0 && x.IsTrial == 0 && x.EndTime >= DateTime.Now)
                .Include(x=>x.Learner)
                .Select(x => x).ToListAsync();
            if (lessionsOfTeacher == null)
            {
                return NotFound("No available courses right now.");
            }

            List<Lesson> oneToOneLessons = SeparateCourseIntoGroups(lessionsOfTeacher).Item1;
            List<Lesson> groupLessons = SeparateCourseIntoGroups(lessionsOfTeacher).Item1;

            result.Data = oneToOneLessons.Concat(groupLessons);
            return Ok(result);
        }

        //GET: http://localhost:5000/api/Chat/GetRelatedTeacher/:userId
        // Get teacherList
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetRelatedTeacher(int id)
        {
            Result<List<Teacher>> result = new Result<List<Teacher>>();
            var learnerDetail = await _ablemusicContext.Learner.Where(x => x.UserId == id).FirstOrDefaultAsync();
            if (learnerDetail == null)
            {
                return NotFound("No such a learner");
            }

            var teachers = await _ablemusicContext.Lesson.Where(x => x.LearnerId == learnerDetail.LearnerId).Select(x => x).ToListAsync();
            if (teachers == null)
            {
                return NotFound("No teachers yet");
            }

            // Remove the duplicated teachdId
            List<int?> teacherIdList = new List<int?>();
            foreach (var teacher in teachers)
            {
                if (!teacherIdList.Contains(teacher.TeacherId))
                {
                    teacherIdList.Add(teacher.TeacherId);
                }
            }

            List<Teacher> teacherList = new List<Teacher>();
            foreach (var Id in teacherIdList)
            {
                var detail = await _ablemusicContext.Teacher.Where(x => x.TeacherId == Id)
                    .FirstOrDefaultAsync();
                if (detail != null)
                {
                    teacherList.Add(detail);
                }
                else
                {
                    return NotFound("Teacher not found");
                }
            }

            if (teacherList.Count == 0)
            {
                return NotFound("No teachers yet");
            }

            result.Data = teacherList;
            return Ok(result);
        }

        // Post new message to database
        [HttpPost]
        public async Task<IActionResult> Post(ChatMessageModel chatMessageModel)
        {
            Result<ChatMessageModel> result = new Result<ChatMessageModel>();
            chatMessageModel.CreateAt = DateTime.Now;
            ChatMessage chatMessage = new ChatMessage();
            try
            {
                _mapper.Map(chatMessageModel, chatMessage);
                await _ablemusicContext.ChatMessage.AddAsync(chatMessage);
                await _ablemusicContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            result.Data = chatMessageModel;
            return Ok(result);
        }

        // Get userId and role from front-end
        // find available courses => before the end time, not a trial, not canceled
        // if role = tutor, => getRelatedStudents (only find the available courses)
        // if role = student, => getRelatedTeachers (only find the available courses)
        // if role = stuff, => get all the available courses of same org

        public Tuple<List<Lesson>, List<Lesson>> SeparateCourseIntoGroups(List<Lesson> lessonList)
        {
            List<Lesson> oneToOneLessons = new List<Lesson>();
            List<Lesson> groupLessons = new List<Lesson>();
            foreach (var lesson in lessonList)
            {
                if (lesson.CourseInstanceId != null)
                {
                    oneToOneLessons.Add(lesson);
                }

                if (lesson.GroupCourseInstance != null && !groupLessons.Contains(lesson))
                {
                    groupLessons.Add(lesson);
                }
            }
            return Tuple.Create(oneToOneLessons, groupLessons);
        }
    }
}