using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Tickets;
using HelpDesk.Services;
using HelpDesk.Models.Users;
using HelpDesk.Models.Clients;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.Models.Task;

namespace HelpDesk.Controllers.Tasks
{
    [Authorize(Roles = "admin,user,superuser")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskCommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskCommentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTaskComments([FromQuery] int? taskId)
        {
            if (taskId == null)
            {
                return BadRequest("Task ID is required!");
            }

            var task = _context
                .TicketTasks
                .Include(x => x.User)
                .FirstOrDefault(x => x.Id == taskId);

            if (task == null)
            {
                return NotFound("Task with such id doesn't exists!");
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTaskByRole(role, task, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            var taskComments = IncludeAllComments().Where(x => x.TaskId == taskId);
            
            return new JsonResult(taskComments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (taskComment == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTaskByRole(role, taskComment.TicketTask, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            return new JsonResult(taskComment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskComment([FromRoute] int id, [FromQuery] string content)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (content == null)
            {
                return BadRequest("Content is required!");
            }

            var taskComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (taskComment == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                if (taskComment.User.Email != User.Identity.Name || taskComment.User == null)
                {
                    return BadRequest();
                }
            }

            taskComment.Content = content;
            _context.Entry(taskComment).State = EntityState.Modified;
         
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostTaskComment([FromBody] TaskComment taskComment)
        {
            if(taskComment.TaskId == 0)
            {
                return BadRequest("Task required!");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = StaticHelper.GetCurrentRole(User);
            var task = await _context.TicketTasks.FirstOrDefaultAsync(x => x.Id == taskComment.TaskId);

            if (task == null)
            {
                return BadRequest();
            }
            if (!StaticHelper.CheckTaskByRole(role, task, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return BadRequest();
            }
            taskComment.UserId = user.Id;
      
            _context.TaskComments.Add(taskComment);
            await _context.SaveChangesAsync();
            await StaticHelper.RaiseEvent(EventTypes.TaskCommented, task, _context);
            return CreatedAtAction("GetTaskComment", new { id = taskComment.Id }, taskComment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var taskComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (taskComment == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                if (taskComment.User.Email != User.Identity.Name || taskComment.User == null)
                {
                    return BadRequest();
                }  
            }

            _context.TaskComments.Remove(taskComment);
            await _context.SaveChangesAsync();

            return new JsonResult(taskComment);
        }

        private IQueryable<TaskComment> IncludeAllComments()
        {
            return _context.TaskComments.Include(x => x.User).Include(x => x.TicketTask).ThenInclude(x => x.User);
        }
    }
}