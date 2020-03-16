using System;
using System.Text;
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
        [HttpGet]
        [Route("{orgId}/{beginDate}/{endDate}")]
        public async Task<ActionResult> GetMessagesByDate(short orgId,DateTime beginDate,DateTime endDate)
        {
            Result<Object> result = new Result<object>();
            try
            {
                result.IsSuccess = true;
                result.Data = await _ablemusicContext.Lesson.
                    Include(l => l.LessonMessage)
                    .Where(l =>l.OrgId==orgId && l.LessonMessage.Count()>0 &&
                    (( l.BeginTime >beginDate &&  l.BeginTime >beginDate) ||
                    (l.LessonMessage.FirstOrDefault( lm=>lm.CreatedAt>beginDate && lm.CreatedAt<endDate )!=null)))
                    .Select(l =>
                        new {
                            TeacherId = l.TeacherId,
                            TeacherName = l.Teacher.FirstName,
                            LearnerId =  l.LearnerId,
                            LearnerFirstName =  l.Learner.FirstName,
                            LearnerLastName =  l.Learner.LastName,
                            BeginTime = l.BeginTime,
                            EndTime = l.EndTime,
                            LessonId = l.LessonId,
                            MessageCount = l.LessonMessage.Count(),
                            MessageLastestDate = l.LessonMessage
                                            .OrderByDescending(lm =>lm.CreatedAt)
                                            .FirstOrDefault().CreatedAt,
                        }
                    ).ToArrayAsync();
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
            StringBuilder EmailContent =  new StringBuilder(@"
                                <td align='center' valign='top' id='templateHeader' style='background:#FFFFFF none no-repeat center/cover;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;background-color: #FFFFFF;background-image: none;background-repeat: no-repeat;background-position: center;background-size: cover;border-top: 0;border-bottom: 0;padding-top: 9px;padding-bottom: 0;'>
                        <!--[if (gte mso 9)|(IE)]>
                        <table align='center' border='0' cellspacing='0' cellpadding='0' width='600' style='width:600px;'>
                        <tr>
                        <td align='center' valign='top' width='600' style='width:600px;'>
                        <![endif]-->
                        <table align='center' border='0' cellpadding='0' cellspacing='0' width='100%' class='templateContainer' style='border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;max-width: 600px !important;'>
                            <tr>
                                <td valign='top' class='headerContainer' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnImageBlock' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
            <tbody class='mcnImageBlockOuter'>
                        <tr>
                            <td valign='top' style='padding: 9px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;' class='mcnImageBlockInner'>
                                <table align='left' width='100%' border='0' cellpadding='0' cellspacing='0' class='mcnImageContentContainer' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                                    <tbody><tr>
                                        <td class='mcnImageContent' valign='top' style='padding-right: 9px;padding-left: 9px;padding-top: 0;padding-bottom: 0;text-align: center;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                                            
                                                
                                                    <img align='center' alt='' src='https://mcusercontent.com/29ed8676ac274685bdb472ffb/images/7502ab7a-c966-47e1-b410-93f7a449afd0.png' width='564' style='max-width: 1200px;padding-bottom: 0px;vertical-align: bottom;display: inline !important;border: 1px none;border-radius: 0%;height: auto;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;' class='mcnImage'>
                                                
                                            
                                        </td>
                                    </tr>
                                </tbody></table>
                            </td>
                        </tr>
                </tbody>
            </table><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnCodeBlock' style='border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                <tbody class='mcnTextBlockOuter'>
                    <tr>
                        <td valign='top' class='mcnTextBlockInner' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                            <div style='background-color:#ddd;margin-left:20px;margin-right:20px'>
            <span>
            ");
            string part2=@"</span>
</span>
</div>

            </td>
        </tr>
    </tbody>
</table><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnTextBlock' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
    <tbody class='mcnTextBlockOuter'>
        <tr>
            <td valign='top' class='mcnTextBlockInner' style='padding-top: 9px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
              	<!--[if mso]>
				<table align='left' border='0' cellspacing='0' cellpadding='0' width='100%' style='width:100%;'>
				<tr>
				<![endif]-->
			    
				<!--[if mso]>
				<td valign='top' width='600' style='width:600px;'>
				<![endif]-->
                <table align='left' border='0' cellpadding='0' cellspacing='0' style='max-width: 100%;min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;' width='100%' class='mcnTextContentContainer'>
                    <tbody><tr>
                        
                        <td valign='top' class='mcnTextContent' style='padding: 0px 18px 9px;color: #4CAAD8;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;word-break: break-word;font-family: Helvetica;font-size: 16px;line-height: 150%;text-align: left;'>
                        
                            <h1 style='display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 26px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;text-align: left;'><span style='font-size:16px'><span style='color:#0000CD'>This is an automated email - please do not reply directly to this email. Please click button to reply!</span></span></h1>

                        </td>
                    </tr>
                </tbody></table>
				<!--[if mso]>
				</td>
				<![endif]-->
                
				<!--[if mso]>
				</tr>
				</table>
				<![endif]-->
            </td>
        </tr>
    </tbody>
</table><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnButtonBlock' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
    <tbody class='mcnButtonBlockOuter'>
        <tr>
            <td style='padding-top: 0;padding-right: 18px;padding-bottom: 18px;padding-left: 18px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;' valign='top' align='center' class='mcnButtonBlockInner'>
                <table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnButtonContentContainer' style='border-collapse: separate !important;border: 1px none;border-radius: 9px;background-color: #2BAADF;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                    <tbody>
                        <tr>
                            <td align='center' valign='middle' class='mcnButtonContent' style='font-family: Arial;font-size: 16px;padding: 18px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>";

            string part4=@"                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
    </tbody>
</table></td>
                                        </tr>
                                    </table>
                                    <!--[if (gte mso 9)|(IE)]>
                                    </td>
                                    </tr>
                                    </table>
                                    <![endif]-->
                                </td>
                            </tr>
                            <tr>
                                <td align='center' valign='top' id='templateBody' style='background:#FFFFFF none no-repeat center/cover;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;background-color: #FFFFFF;background-image: none;background-repeat: no-repeat;background-position: center;background-size: cover;border-top: 0;border-bottom: 0;padding-top: 9px;padding-bottom: 9px;'>
                                    <!--[if (gte mso 9)|(IE)]>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' width='600' style='width:600px;'>
                                    <tr>
                                    <td align='center' valign='top' width='600' style='width:600px;'>
                                    <![endif]-->
                                    <table align='center' border='0' cellpadding='0' cellspacing='0' width='100%' class='templateContainer' style='border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;max-width: 600px !important;'>
                                        <tr>
                                            <td valign='top' class='bodyContainer' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'></td>
                                        </tr>
                                    </table>
                                    <!--[if (gte mso 9)|(IE)]>
                                    </td>
                                    </tr>
                                    </table>
                                    <![endif]-->
                                </td>
                            </tr>
                            <tr>
                                <td align='center' valign='top' id='templateFooter' style='background:#FAFAFA none no-repeat center/cover;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;background-color: #FAFAFA;background-image: none;background-repeat: no-repeat;background-position: center;background-size: cover;border-top: 0;border-bottom: 0;padding-top: 9px;padding-bottom: 9px;'>
                                    <!--[if (gte mso 9)|(IE)]>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' width='600' style='width:600px;'>
                                    <tr>
                                    <td align='center' valign='top' width='600' style='width:600px;'>
                                    <![endif]-->
                                    <table align='center' border='0' cellpadding='0' cellspacing='0' width='100%' class='templateContainer' style='border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;max-width: 600px !important;'>
                                        <tr>
                                            <td valign='top' class='footerContainer' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnDividerBlock' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;table-layout: fixed !important;'>
    <tbody class='mcnDividerBlockOuter'>
        <tr>
            <td class='mcnDividerBlockInner' style='min-width: 100%;padding: 10px 18px 25px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                <table class='mcnDividerContent' border='0' cellpadding='0' cellspacing='0' width='100%' style='min-width: 100%;border-top: 2px solid #EEEEEE;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                    <tbody><tr>
                        <td style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
                            <span></span>
                        </td>
                    </tr>
                </tbody></table>
<!--            
                <td class='mcnDividerBlockInner' style='padding: 18px;'>
                <hr class='mcnDividerContent' style='border-bottom-color:none; border-left-color:none; border-right-color:none; border-bottom-width:0; border-left-width:0; border-right-width:0; margin-top:0; margin-right:0; margin-bottom:0; margin-left:0;' />
-->
            </td>
        </tr>
    </tbody>
</table><table border='0' cellpadding='0' cellspacing='0' width='100%' class='mcnTextBlock' style='min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
    <tbody class='mcnTextBlockOuter'>
        <tr>
            <td valign='top' class='mcnTextBlockInner' style='padding-top: 9px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;'>
              	<!--[if mso]>
				<table align='left' border='0' cellspacing='0' cellpadding='0' width='100%' style='width:100%;'>
				<tr>
				<![endif]-->
			    
				<!--[if mso]>
				<td valign='top' width='600' style='width:600px;'>
				<![endif]-->
                <table align='left' border='0' cellpadding='0' cellspacing='0' style='max-width: 100%;min-width: 100%;border-collapse: collapse;mso-table-lspace: 0pt;mso-table-rspace: 0pt;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;' width='100%' class='mcnTextContentContainer'>
                    <tbody><tr>
                        
                        <td valign='top' class='mcnTextContent' style='padding-top: 0;padding-right: 18px;padding-bottom: 9px;padding-left: 18px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;word-break: break-word;color: #656565;font-family: Helvetica;font-size: 12px;line-height: 150%;text-align: center;'>
                        
                            <h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><span style='font-size:20px'><img data-file-id='1270491' height='88' src='https://mcusercontent.com/29ed8676ac274685bdb472ffb/images/ed52b6cd-88f5-41f6-9dec-fd071cf64a60.png' style='border: 0px;width: 91px;height: 88px;margin: 0px;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;' width='91'><br>
ABLE MUSIC STUDIO</span><br>
<br>
<br>
<font size='4'>Central AKL Branch</font></strong></h2>

<div style='text-align: left;'><strong>Address:</strong>&nbsp;Level 2,&nbsp;953 New North Rd, Mt Albert, Auckland<br>
<strong>Ph</strong><strong>one:</strong>&nbsp; &nbsp;09-846 9618<br>
<strong>Fax:</strong>&nbsp; 09-846 9848<br>
<strong>Email:</strong>&nbsp;<a href='mailto:mtalbert@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'>mtalbert@ablemusic.co.nz</a></div>

<h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><font size='4'>East AKL Branch</font></strong></h2>

<div style='text-align: left;'><strong>Add</strong><strong>ress</strong><strong>:</strong>&nbsp;2/ 550 Chapel Rd, Botany, Auckland<br>
<strong>Ph</strong><strong>one</strong><strong>:</strong>&nbsp; 09-274 9618<br>
<strong>Fax:</strong>&nbsp; 09-274 9648<br>
<strong>Email:</strong>&nbsp;<a href='mailto:botany@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'>botany@ablemusic.co.nz</a></div>

<h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><font color='#2a2a2a' size='4'>Auckland City Branch</font></strong></h2>

<div style='text-align: left;'><font size='2'><strong>Add</strong><strong>ress</strong><strong>:</strong>&nbsp;</font><font size='2'>Level 5, 109 Queens St, Auckland CBD</font><br>
<strong>Ph</strong><strong>one</strong><strong>:</strong>&nbsp; 09-377 9618<br>
<strong>​Fax:&nbsp;</strong>​09-377 9648<br>
<strong>Email:</strong>&nbsp;<a href='mailto:aklcity@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'>aklcity@ablemusic.co.nz</a></div>

<h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><font size='4'>Henderson Branch</font></strong></h2>

<div style='text-align: left;'><strong>Address:</strong>&nbsp;Level 2, 34 Te Pai Place, Henderson, Auckland<br>
<strong>Ph</strong><strong>one:</strong>&nbsp; &nbsp;09-835 9618<br>
<strong>Fax:</strong>&nbsp; 09-835 9648<br>
<strong>Email:</strong><font size='1'>&nbsp;</font><a href='http://henderson@ablemusic.co.nz/' target='_blank' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'><font size='2'>henderson@ablemusic.co.nz</font><font size='2'>&nbsp;</font></a></div>

<h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><font size='4'><font size='4'>North Shore Branch</font></font></strong></h2>

<div style='text-align: left;'><strong>Add</strong><strong>ress</strong><strong>:&nbsp;</strong><font size='2'>Level 2, 8C Link Drive, Sunnynook, Auckland</font><br>
<strong>Ph</strong><strong>one</strong><strong>:&nbsp;</strong><font size='2'>09-444 6888</font><br>
<strong>Fax:&nbsp;</strong><font size='2'>09-444 9648</font><br>
<strong><font size='2'>Email</font><font size='2'>:&nbsp;</font></strong><a href='mailto:sunnynook@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'><font size='2'>sunnynook@ablemusic.co.nz</font></a></div>

<h2 style='text-align: left;display: block;margin: 0;padding: 0;color: #202020;font-family: Helvetica;font-size: 22px;font-style: normal;font-weight: bold;line-height: 125%;letter-spacing: normal;'><strong><font size='4'><font size='4'>Epsom Branch</font></font></strong></h2>

<div style='text-align: left;'><strong>Add</strong><strong>ress</strong><strong>:&nbsp;</strong><font size='2'>Level 2, 123 Manukau Road, Epsom, Auckland</font><br>
<strong>Ph</strong><strong>one</strong><strong>:&nbsp;</strong><font size='2'>09-522 9618&nbsp;</font><br>
<strong>Fax:&nbsp;</strong><font size='2'>09-522 9648</font><br>
<strong><font size='2'>Email</font><font size='2'>:&nbsp;</font></strong><font size='2'><a href='mailto:epsom@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'>epsom@ablemusic.co.nz</a></font><br>
<br>
<strong><font size='4'><font size='4'>Auckland Music Academy</font></font></strong><br>
<strong>Add</strong><strong>ress</strong><strong>:&nbsp;</strong>19 William Roberts Rd, Pakuranga, Auckland<br>
<strong>Ph</strong><strong>one</strong><strong>:&nbsp;</strong><font size='2'>09-577 3311</font><br>
<strong><font size='2'>Email</font><font size='2'>:<u>&nbsp;info</u></font></strong><u><font size='2'><a href='mailto:sunnynook@ablemusic.co.nz' style='mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;color: #656565;font-weight: normal;text-decoration: underline;'>@</a>nzama.co.nz</font></u><br>
<br>
<br>
<em>Copyright © 2020&nbsp; ABLE MUSIC STUDIO All rights reserved.</em></div>";                            
            string title = "Message For Able Music Studio Lesson("+lesson.BeginTime.Value.ToString("MM/dd/yyyy H:mm")+")";

            
            string userConfirmUrlPrefix = _configuration.GetSection("UrlPrefixMessage").Value;
            string photoUrlPrefix = _configuration.GetSection("UrlPrefixPhoto").Value;
		                
            string confirmURLTeacher=userConfirmUrlPrefix+"Messgeboard/1/"+lesson.LessonId+"/"+lesson.TeacherId;
            string confirmURLStudent=userConfirmUrlPrefix+"Messgeboard/4/"+lesson.LessonId+"/"+lesson.LearnerId;
            
      
             //role  1:teacher 3:Receptionist,manager,principal 4:learner
             try
             {
                if (role==4){ //Teacher  Send to student
                    var teacher = _ablemusicContext.Teacher.FirstOrDefault(t => t.TeacherId==lesson.TeacherId);

                    string teacherImg;
                    if (teacher!=null && teacher.Photo!=null)
                        teacherImg=photoUrlPrefix+teacher.Photo;
                    else
                        teacherImg="https://d3fa68hw0m2vcc.cloudfront.net/931/213379411.jpeg";
                    
                    var teacherContent = "<img src='"+teacherImg+"' alt='icon' style='max-width: 60px;border-radius: 60%;margin-left: 10px;border: 0;height: auto;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;'> "
                        +"<span style='margin-left:20px;margin-right:20px;font:12px/1 'Merriweather', arial, sans-serif'><div style='color:blue'>"
                        +teacher.FirstName+" "+teacher.LastName+"</div>"+content;
                    EmailContent.Append(teacherContent);
                    EmailContent.Append(part2);
                    var urlTeacher=
                        "<a class='mcnButton ' title='Reply' href='"+confirmURLTeacher+"' target='_blank' style='font-weight: bold;letter-spacing: normal;line-height: 100%;text-align: center;text-decoration: none;color: #FFFFFF;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;display: block;'>Reply</a>";
                    EmailContent.Append(urlTeacher);
                    EmailContent.Append(part4);
                    return MailSenderService.SendMail(teacherEmail,title,EmailContent.ToString());

                }else if (role==1){ //Student Send to Teacher
                    var learner = _ablemusicContext.Learner.FirstOrDefault(t => t.LearnerId==lesson.LearnerId);
                    var studentContent = "<img src='http://www.ablemusic.co.nz/uploads/4/4/4/0/44407777/2908727_orig.jpg' alt='icon' style='max-width: 60px;border-radius: 60%;margin-left: 10px;border: 0;height: auto;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;'> "
                        +"<span style='margin-left:20px;margin-right:20px;font:12px/1 'Merriweather', arial, sans-serif'><div style='color:blue'>"
                        +learner.FirstName+" "+learner.LastName+"</div>"+content;
                    EmailContent.Append(studentContent);
                    EmailContent.Append(part2);
                    var urlStudent=
                        "<a class='mcnButton ' title='Reply' href='"+confirmURLStudent+"' target='_blank' style='font-weight: bold;letter-spacing: normal;line-height: 100%;text-align: center;text-decoration: none;color: #FFFFFF;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;display: block;'>Reply</a>";
                    EmailContent.Append(urlStudent);
                    EmailContent.Append(part4);
                    return MailSenderService.SendMail(learnerEmail,title,EmailContent.ToString());
                }
                else{

                    var allContent = "<img src='"+"http://www.gradspace.org:8888/assets/images/Avatar/able.png"+"' alt='icon' style='max-width: 60px;border-radius: 60%;margin-left: 10px;border: 0;height: auto;outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;'> "
                        +"<span style='margin-left:20px;margin-right:20px;font:12px/1 'Merriweather', arial, sans-serif'><div style='color:blue'>"
                        +"ABLE MUSIC STUDIO RECEIPTIONIST"+"</div>"+content;
                    EmailContent.Append(allContent);
                    EmailContent.Append(part2);
                    

                     var urlTeacher=
                         "<a class='mcnButton ' title='Reply' href='"+confirmURLTeacher+"' target='_blank' style='font-weight: bold;letter-spacing: normal;line-height: 100%;text-align: center;text-decoration: none;color: #FFFFFF;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;display: block;'>Reply</a>";
                     var urlLearner=
                         "<a class='mcnButton ' title='Reply' href='"+confirmURLStudent+"' target='_blank' style='font-weight: bold;letter-spacing: normal;line-height: 100%;text-align: center;text-decoration: none;color: #FFFFFF;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%;display: block;'>Reply</a>";

                    // EmailContent.Append(urlTeacher);
                    // EmailContent.Append(part4);
                    var teacherContent = EmailContent+urlTeacher+part4;
                    var learnerContent = EmailContent+urlLearner+part4;
                    if (MailSenderService.SendMail(learnerEmail,title,learnerContent)==false )
                        return false;    
                    return MailSenderService.SendMail(teacherEmail,title,teacherContent );                                                           
                }
                    
             }
             catch (Exception e)
             {
                 throw e;
             }
        }
    }
}