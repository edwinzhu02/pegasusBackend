using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pegasus_backend.Models;
using Pegasus_backend.pegasusContext;
using AutoMapper;
namespace Pegasus_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentController : BasicController
    {   
        private IMapper _mapper;
        public ParentController(ablemusicContext ablemusicContext, IMapper mapper,ILogger<PageController> log) : base(ablemusicContext, log)
        {
            _mapper = mapper;
        }

        [HttpGet("{learnerId}")]
        public async Task<IActionResult> GetParentByLearnerID(int learnerId)
        {
            var result = new Result<Object>();
            try
            {

                var item = await _ablemusicContext.Parent.ToListAsync();
                result.Data = item;
                result.IsSuccess = true;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return BadRequest(result);
            }
        }
        [HttpPost("{learnerId}")]
        public async Task<IActionResult> PostParents(int learnerId,[FromBody] List<ParentView> ParentsView)
        {
            var result = new Result<Object>();

            List<Parent> Parents = new List<Parent>();
            _mapper.Map(ParentsView, Parents);

            try
            {
                foreach(var parent in Parents) {
                    parent.LearnerId = learnerId;
                    await _ablemusicContext.Parent.AddAsync(parent);
                }

                await _ablemusicContext.SaveChangesAsync();
            }

            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParents(int id)
        {
            var result = new Result<Object>();
            Parent parent = new Parent();
            try
            {
                parent = await _ablemusicContext.Parent
                .Where(s => s.ParentId == id).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            if (parent == null)
            {
                result.ErrorMessage = "The parent id is not exists";
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }

            try
            {
                _ablemusicContext.Parent.Remove(parent);
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

        [HttpPut("{learnerId}")]
        public async Task<IActionResult> PutParents(int learnerId ,[FromBody]  List<ParentView> ParentsView)
        {
            var result = new Result<string>();
            List<Parent> Parents = new List<Parent>();
            
            var ParentsExist =  _ablemusicContext.Parent.Where(p => p.LearnerId == learnerId );

            
            foreach(var parent in ParentsExist) {
                _ablemusicContext.Parent.Remove(parent);
            }
            _mapper.Map(ParentsView, Parents);

            try
            {
                foreach(var parent in Parents) {
                    parent.LearnerId = learnerId;
                    await _ablemusicContext.Parent.AddAsync(parent);
                }

                await _ablemusicContext.SaveChangesAsync();
            }

            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.IsSuccess = false;
                result.IsFound = false;
                return BadRequest(result);
            }
            return Ok(result);

        }

    }
}

