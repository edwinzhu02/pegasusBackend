/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : BasicController
    {
        private readonly pegasusContext.ablemusicContext _pegasusContext;
        private readonly IMapper _mapper;

        public SessionController(pegasusContext.ablemusicContext pegasusContext, IMapper mapper)
        {
            _pegasusContext = pegasusContext;
            _mapper = mapper;
        }

        // GET: api/Session
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetSession()
        {
            Result<List<Lesson>> result = new Result<List<Lesson>>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _pegasusContext.Lesson.Include(x => x.CourseInstance).Include(x => x.GroupCourseInstance).Include(x => x.Learner).Include(x => x.Org).Include(x => x.Room).Include(x => x.Teacher).ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        // PUT: api/Session/5
        [HttpPut("{id}/{reason}")]
        public async Task<IActionResult> PutSesson(int id, string reason)
        {
            var result = new Result<string>();
            Lesson lesson = new Lesson();

            var updateSession = await _pegasusContext.Lesson.Where(x => x.LessonId == id).FirstOrDefaultAsync();
            if (updateSession == null)
            {
                return NotFound(DataNotFound(result));
            }
            lesson = updateSession;
            lesson.IsCanceled = 1;
            lesson.Reason = reason;
            UpdateTable(lesson, typeof(Lesson), updateSession);

            TodoList todolist = new TodoList();
            todolist.ListName = "listName";                  //rules?
            todolist.ListContent = "list content";           //rules?
            todolist.CreatedAt = DateTime.Now;
            todolist.ProcessedAt = DateTime.Now;             //when?
            todolist.ProcessFlag = 0;
            todolist.UserId = 0;                            //where it come from?
            todolist.TodoDate = DateTime.Now;               //what is this?

            RemindLog remindLog = new RemindLog();
            remindLog.LearnerId = lesson.LearnerId;
            remindLog.Email = "";                            //query?
            remindLog.RemindType = 0;                        //rules?
            remindLog.RemindContent = "remind content";      //rules?
            remindLog.CreatedAt = DateTime.Now;
            remindLog.TeacherId = lesson.TeacherId;
            remindLog.IsLearner = 0;                         //how?
            remindLog.ProcessFlag = 0;
            remindLog.EmailAt = DateTime.Now;               //empty?


            try
            {
                await _pegasusContext.TodoList.AddAsync(todolist);
                await _pegasusContext.RemindLog.AddAsync(remindLog);
                await _pegasusContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);

        }

        private bool LessonExists(int id)
        {
            return _pegasusContext.Lesson.Any(e => e.LessonId == id);
        }
    }
}*/