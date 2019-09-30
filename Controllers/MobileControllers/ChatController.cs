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

        public ChatController(ablemusicContext ablemusicContext, ILogger<NavItemsController> log, IHubContext<Chatroom> chatRoom, IMapper mapper) : base(ablemusicContext, log)
        {
            _chatRoom = chatRoom;
            _mapper = mapper;
        }


        // Post new message to database
        [HttpPost]
        public async Task<IActionResult> Post(ChatMessageModel chatMessageModel)
        {
            Result<ChatMessageModel> result = new Result<ChatMessageModel>();
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

        // GET: http://localhost:5000/api/Chat/PostOfflineMessage/userId
        [Route("[action]/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetOfflineMessage(int id)
        {
            var result = new Result<List<ChatMessage>>();
            var user = await _ablemusicContext.User.Where(x => x.UserId == id).FirstOrDefaultAsync();
            var unreadId = user.UnreadMessageId;
            // no unread message
            if (unreadId == null)
            {
                result.Data = null;
                return Ok(result);
            }

            var unreadList = await _ablemusicContext.ChatMessage.Take(unreadId.Value).Where(x => x.ReceiverUserId == id)
                .ToListAsync();
            result.Data = unreadList;
            return Ok(result);
        }

        // POST: http://localhost:5000/api/Chat/PostOfflineMessage
        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> PostOfflineMessage(ChatMessageModel chatMessageModel)
        {
            Result<ChatMessageModel> result = new Result<ChatMessageModel>();
            ChatMessage chatMessage = new ChatMessage();
            try
            {
                var receiver = await _ablemusicContext.User.Where(x => x.UserId == chatMessageModel.ReceiverUserId)
                    .FirstOrDefaultAsync();
                if (receiver == null)
                {
                    result.ErrorMessage = "Can not find the message receiver.";
                    return NotFound(result);
                }
                _mapper.Map(chatMessageModel, chatMessage);
                await _ablemusicContext.ChatMessage.AddAsync(chatMessage);
                await _ablemusicContext.SaveChangesAsync();
                receiver.UnreadMessageId = chatMessage.ChatMessageId;
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

        private async Task<IList> GetAllStaff(int? staffId)
        {
            if (staffId == null)
            {
               return await _ablemusicContext.Staff.Select(x => new
                   {x.FirstName, x.LastName, x.UserId, x.Photo}).ToListAsync();
            }
            return await _ablemusicContext.Staff.Where(x=>x.StaffId != staffId).Select(x => new
            {x.FirstName, x.LastName, x.UserId, x.Photo}).ToListAsync();
        }

        private async Task<IList> GetAllTeacher()
        {
            return await _ablemusicContext.Teacher.Select(x => new  
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
            var studentsIdHavingLesson = await _ablemusicContext.Lesson.Where(x => x.OrgId == orgIdOfStaff)
                .Include(x=>x.Learner).Select(x=> new
                {
                    x.Learner.LearnerId,
                    x.Learner.FirstName,
                    x.Learner.LastName,
                    x.Learner.UserId,
                    x.Learner.Photo
                }).Where(x=>x.UserId !=null).Distinct().ToListAsync();

            // students registered in his org
            var studentsRegisteredIn = await _ablemusicContext.Learner.Where(x=>x.OrgId == orgIdOfStaff).Select(x => new
            {
                x.LearnerId,
                x.FirstName,
                x.LastName,
                x.UserId,
                x.Photo
            }).Where(x=>x.UserId !=null).Distinct().ToListAsync();

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