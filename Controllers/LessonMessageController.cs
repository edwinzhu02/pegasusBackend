using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Pegasus_backend.pegasusContext;
using Pegasus_backend.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Pegasus_backend.Controllers;
using Microsoft.Extensions.Configuration;
using Pegasus_backend.ActionFilter;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Services;
using Pegasus_backend.Utilities;

namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonMessageController : BasicController
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public LessonMessageController(ablemusicContext ablemusicContext, ILogger<ProductController> log, IMapper mapper,IConfiguration configuration) : base(ablemusicContext, log)
        {
            _mapper = mapper;
            _configuration = configuration;
        }


        //GET: http://localhost:5000/api/product/{id}
        [HttpGet]
        [Route("{lessonId}")]
        public async Task<ActionResult> GetMessages(int lessonId)
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.LessonMessage
                    .Include(l => l.Teacher).Include(l => l.Learner)
                    .Where(lm =>lm.LessonId==lessonId).OrderByDescending(lm =>lm.MessageId)
                    .ToListAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }

        [HttpPost]
        [CheckModelFilter]
        public async Task<ActionResult<Product>> PostMessage(LessonMsgPost msgModel)
        {
            //Result<List<Product>> result = new Result<List<Product>>();
            var result = new Result<object>();
            var lessonMessage = new LessonMessage();
            //role  1:teacher 3:Receptionist,manager,principal 4:learner
            if (msgModel.role==1){
                lessonMessage.TeacherId =(short) msgModel.Id;
            }else if (msgModel.role==4){
                lessonMessage.LearnerId =(short) msgModel.Id;
            }
            else{
                lessonMessage.StaffId =(short) msgModel.Id;
            }

            var lesson =  await _ablemusicContext.Lesson.
                FirstOrDefaultAsync(l =>l.LessonId ==msgModel.LessonId);
            var learner = await _ablemusicContext.Learner.
                FirstOrDefaultAsync(l =>l.LearnerId ==lesson.LearnerId);
            var teacher = await _ablemusicContext.Teacher.
                FirstOrDefaultAsync(l =>l.TeacherId ==lesson.TeacherId);   

                lessonMessage.Message=msgModel.MessageContent;
                lessonMessage.LessonId=msgModel.LessonId;
                lessonMessage.LearnerEmail=learner.Email;                             
                lessonMessage.TeacherEmail=teacher.Email;
                lessonMessage.CreatedAt = DateTime.UtcNow.ToNZTimezone();                                             
            try
            {
                await _ablemusicContext.LessonMessage.AddAsync(lessonMessage);
                if (PackageEmailContent(msgModel.role,msgModel.MessageContent,lesson,learner.Email,teacher.Email)==false )
                        throw new Exception("Email Send Error!");
                await _ablemusicContext.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                return BadRequest(result);
            }

            return Ok(result);

        }
        private Boolean PackageEmailContent(short role,string content,Lesson lesson,string learnerEmail,string teacherEmail )
        {
            string title = "Message For Able Music Studio Lesson("+lesson.BeginTime.Value.ToString("MM/dd/yyyy H:mm")+")";
            string tableBegin = "<img src='http://www.ablemusic.co.nz/uploads/4/4/4/0/44407777/1434783687.png' alt='sunil'> <table style='width:80%'>";
            string tableEnd = "</table>";
            string mailContent = "<tr><td></td></tr><tr><td>"+content+"</td></tr><tr><td></td></tr>";
// Messgeboard/:role/:lessonId/:id
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefixMessage").Value;
		                
            string confirmURLTeacher=userConfirmUrlPrefix+"Messgeboard/1/"+lesson.LessonId+"/"+lesson.TeacherId;
            string confirmURLStudent=userConfirmUrlPrefix+"Messgeboard/4/"+lesson.LessonId+"/"+lesson.LearnerId;
            
            string teacherButton="<tr><td style='text-align:center;background-color:#2fb7ec'><a href='" + confirmURLTeacher + "' target='_blank'>Rely</a></td></tr>";
            string studentButton="<tr><td style='text-align:center;background-color:#2fb7ec'><a href='" + confirmURLStudent + "' target='_blank'>Rely</a></td></tr>";                                 
            string style =@"<style>
                    a{
                    background-color: red;
                    box-shadow: 0 5px 0 darkred;
                    color: white;
                    padding: 1em 1.5em;
                    position: relative;
                    text-align:center;
                    text-decoration: none;
                    text-transform: uppercase;
                    display: inline-block;
                    width: 200px;
                    }

                    a:hover {
                    background-color: #ce0606;
                    cursor: pointer;
                    }

                    a:active {
                    box-shadow: none;
                    top: 5px;
                    }
                    p  {
                    color: blue;
                    font-family: courier;
                    font-size: 160%;
                    }
            </style>";
            style ="";
            // mailContent = mailContent+style;
            // string studentButton="<button type='button' (click)='window.location.assign('https://www.w3schools.com')'>Goto</button>";
             //role  1:teacher 3:Receptionist,manager,principal 4:learner
             try
             {
                if (role==1){
                    mailContent ="<tr><td style='coler:blue'>This Message is From Teacher:</tr></td>"+mailContent;
                    mailContent = tableBegin+mailContent+teacherButton+tableEnd+style;
                    return MailSenderService.SendMail(teacherEmail,title,mailContent);
                }else if (role==4){
                    mailContent ="<tr><td style='coler:blue'>This Message is From Student</tr></td>"+mailContent; 
                    mailContent = tableBegin+mailContent+studentButton+tableEnd+style; 
                    return MailSenderService.SendMail(learnerEmail,title,mailContent);                                      
                }
                else{
                    mailContent ="<tr><td style='coler:blue'>This Message is From School</tr></td>"+mailContent; 
                    var mailContentStudent = tableBegin+mailContent+studentButton+tableEnd+style; 
                    if (MailSenderService.SendMail(learnerEmail,title,mailContentStudent)==false )
                        return false;    
                    var mailContentTeacher = mailContent+teacherButton+tableEnd+style;
                    return MailSenderService.SendMail(teacherEmail,title,mailContentTeacher );                                                           
                }
                    
             }
             catch (Exception e)
             {
                 throw e;
             }
        }
    }
}