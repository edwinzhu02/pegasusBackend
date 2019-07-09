using System;
using System.Collections;
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
        private readonly IHubContext<Chatroom> _chatRoom;

        public ChatController(ablemusicContext ablemusicContext, ILogger<NavItemsController> log, IHubContext<Chatroom> chatRoom) : base(ablemusicContext, log)
        {
            _chatRoom = chatRoom;
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> TestMessage(ChatMessageModel chatMessageModel)
        {
            return Ok(chatMessageModel);
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

        // pass in userId
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetStaffChattingList(int id)
        {
            //validate role
            var staffDetial = await _ablemusicContext.Staff.Where(x => x.UserId == id).FirstOrDefaultAsync();
            if (staffDetial == null)
            {
                return BadRequest("Only staff can access this list.");
            }
            // find org of this stuff
            var orgIdOfStaff = await _ablemusicContext.StaffOrg.Where(x => x.StaffId == staffDetial.StaffId).Select(x=>x.OrgId)
                .FirstOrDefaultAsync();
            if (orgIdOfStaff == null)
            {
                return NotFound("Staff's orgId not found");
            }

            // all the staff except himself
            List<Staff> staffList = new List<Staff>();
            staffList = await _ablemusicContext.Staff.Where(x=>x.UserId != id).Take(3).ToListAsync();
            
            // all teache teachers
            //List<Teacher> teacherList = new List<Teacher>();
            var teacherList = await _ablemusicContext.Teacher.Take(3).Select(x => new Teacher 
                    {TeacherId = x.TeacherId, FirstName = x.FirstName, LastName = x.LastName, Email = x.Email})
                .ToListAsync();

            // Students having lesson in his org
            var studentsIdHavingLesson = await _ablemusicContext.Lesson.Where(x => x.OrgId == orgIdOfStaff).Take(3)
                .Include(x=>x.Learner).Select(x => new Lesson 
                    {LessonId = x.LessonId, LearnerId = x.LearnerId, Learner = x.Learner})
                .ToListAsync();

            // students registered in his org
            var studentsRegisteredIn = await _ablemusicContext.Learner.Where(x=>x.OrgId == orgIdOfStaff).Take(3).ToListAsync();

            // combine data
            Result<Tuple<List<Staff>, List<Teacher>, List<Lesson>, List<Learner>>> result = new Result<Tuple<List<Staff>, List<Teacher>, List<Lesson>, List<Learner>>>
            {
                Data = Tuple.Create(staffList, teacherList, studentsIdHavingLesson, studentsRegisteredIn)
            };
            return Ok(result);
        }

        // http://localhost:5000/api/Chat/GetChattingList/:userId
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetChattingList(int id)
        {
            Result<ChatListModel> result = new Result<ChatListModel>();

            //validate role
            var userDetail = await _ablemusicContext.User.Where(x => x.UserId == id).Include(x=>x.Role).FirstOrDefaultAsync();
            if (userDetail.RoleId == null)
            {
                return NotFound("Can not find roleId");
            }
            switch (userDetail.Role.RoleName)
            {
                case "teacher":
                    var teacherDetail = await _ablemusicContext.Teacher.Where(x => x.UserId == id).FirstOrDefaultAsync();
                    if (teacherDetail == null)
                    {
                        return NotFound("Can not find teacherId");
                    }
                    else
                    {
                        try
                        {
                            result.Data = await GetChatListOfTeacher(teacherDetail.TeacherId);
                        }
                        catch (Exception e)
                        {
                            result.ErrorMessage = e.Message;
                            return BadRequest(result);
                        }

                        return Ok(result);
                    }
                case "learner":
                    var learnerDetail = await _ablemusicContext.Learner.Where(x => x.UserId == id).FirstOrDefaultAsync();
                    if (learnerDetail == null)
                    {
                        return NotFound("Can not find learnerId");
                    }
                    else
                    {
                        try
                        {
                            result.Data = await GetChatListOfLearner(learnerDetail.LearnerId);
                        }
                        catch (Exception e)
                        {
                            result.ErrorMessage = e.Message;
                            return BadRequest(result);
                        }

                        return Ok(result);
                    }
                // get staff chatting list
                default:
                    var staffDetail = await _ablemusicContext.Staff.Where(x => x.UserId == id).FirstOrDefaultAsync();
                    if (staffDetail == null)
                    {
                        return NotFound("Can not find staffId");
                    }
                    else
                    {
                        try
                        {
                            result.Data = await GetChatListOfStaff(staffDetail.StaffId);
                        }
                        catch (Exception e)
                        {
                            result.ErrorMessage = e.Message;
                            return BadRequest(result);
                        }
                    }
                    return Ok(result);
            }
        }
       
        private async Task<IList> GetAllStaff(int? staffId)
        {
            if (staffId == null)
            {
               return await _ablemusicContext.Staff.Take(3).Select(x => new
                   {x.FirstName, x.LastName, x.UserId, x.Photo}).ToListAsync();
            }
            return await _ablemusicContext.Staff.Where(x=>x.StaffId != staffId).Take(3).Select(x => new
            {x.FirstName, x.LastName, x.UserId, x.Photo}).ToListAsync();
        }

        private async Task<IList> GetAllTeacher()
        {
            return await _ablemusicContext.Teacher.Take(5).Select(x => new  
                    {x.TeacherId, x.FirstName, x.LastName, x.Photo, x.UserId}).ToListAsync();
        }

        private async Task<ChatListModel> GetChatListOfLearner(int learnerId)
        {
            var lessonsOfLearner = await _ablemusicContext.Lesson
                .Where(x => x.LearnerId == learnerId && x.IsCanceled == 0 && x.IsTrial == 0 && x.EndTime >= DateTime.Now)
                .Include(x=>x.Learner)
                .Select(x => new
                {
                    x.LessonId, x.LearnerId, x.OrgId, x.TeacherId, x.CourseInstanceId, x.GroupCourseInstanceId
                }).ToListAsync();
            if (lessonsOfLearner == null)
            {
                throw new Exception("No courses for this learner at the moment");
            }

            // add data to return
            var lessonList = SeparateCourseIntoGroups(lessonsOfLearner);
            ChatListModel chatList = new ChatListModel
            {
                OneToOneCourseList = lessonList["oneToOneLessons"],
                OneToManyCourseList = lessonList["groupLessons"],
                StaffList = await GetAllStaff(null)
            };
            return chatList;
        }

        private async Task<ChatListModel> GetChatListOfTeacher(int teacherId)
        {
            // find available courses of teacher
            var lessonsOfTeacher = await _ablemusicContext.Lesson
                .Where(x => x.TeacherId == teacherId && x.IsCanceled == 0 && x.IsTrial == 0 && x.EndTime >= DateTime.Now)
                .Select(x => new
                {
                    x.LessonId, x.LearnerId, x.OrgId, x.TeacherId, x.CourseInstanceId, x.GroupCourseInstanceId
                })
                .ToListAsync();
            if (lessonsOfTeacher == null)
            {
                throw new Exception("No courses for this teacher right now");
            }

            // add data to return
            var lessonList = SeparateCourseIntoGroups(lessonsOfTeacher);
            ChatListModel chatList = new ChatListModel
            {
                OneToOneCourseList = lessonList["oneToOneLessons"],
                OneToManyCourseList = lessonList["groupLessons"],
                StaffList = await GetAllStaff(null)
            };
            return chatList;
        }

        private Dictionary<string, IList> SeparateCourseIntoGroups(IList lessonList)
        {
            Dictionary<string, IList> allLessons = new Dictionary<string, IList>();
            List<dynamic> oneToOneLessons = new List<dynamic>();
            List<dynamic> groupLessons = new List<dynamic>();
            foreach (var lesson in lessonList)
            {
                if (lesson.GetType().GetProperty("CourseInstanceId").GetValue(lesson) != null)
                {
                    oneToOneLessons.Add(lesson);
                }

                if (lesson.GetType().GetProperty("GroupCourseInstanceId").GetValue(lesson) != null)
                {
                    groupLessons.Add(lesson);
                }
            }
            allLessons.Add("oneToOneLessons", oneToOneLessons);
            allLessons.Add("groupLessons", groupLessons);
            return allLessons;
        }

        private async Task<ChatListModel> GetChatListOfStaff(int staffId)
        {
            // find org of this stuff
            var orgIdOfStaff = await _ablemusicContext.StaffOrg.Where(x => x.StaffId == staffId).Select(x=>x.OrgId)
                .FirstOrDefaultAsync();
            if (orgIdOfStaff == null)
            {
                throw new Exception("Staff's orgId not found");
            }

            // Students having lesson in his org
            var studentsIdHavingLesson = await _ablemusicContext.Lesson.Where(x => x.OrgId == orgIdOfStaff).Take(5)
                .Include(x=>x.Learner).Select(x=> new
                {
                    x.Learner.LearnerId,
                    x.Learner.FirstName,
                    x.Learner.LastName,
                    x.Learner.UserId,
                    x.Learner.Photo
                }).Distinct().ToListAsync();

            // students registered in his org
            var studentsRegisteredIn = await _ablemusicContext.Learner.Where(x=>x.OrgId == orgIdOfStaff).Take(5).Select(x => new
            {
                x.LearnerId,
                x.FirstName,
                x.LastName,
                x.UserId,
                x.Photo
            }).Distinct().ToListAsync();

            // combine data
            ChatListModel chatListModel = new ChatListModel
            {
                StaffList = await GetAllStaff(staffId),
                TeacherList = await GetAllTeacher(),
                LearnerList = studentsIdHavingLesson.Union(studentsRegisteredIn).Distinct().ToList()
            };
            return chatListModel;
        }
    }
}