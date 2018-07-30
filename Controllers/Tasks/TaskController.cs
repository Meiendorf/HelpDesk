using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Task;
using HelpDesk.Models.Users;
using HelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers.Tasks
{
    [Authorize(Roles = "admin,user,superuser")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private AppDbContext db { get; set; }

        public TaskController(AppDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketTask([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var user = await db.Users.Include(x => x.Role)
                               .Include(x => x.Departament)
                               .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            var task = await IncludeAllTasks().FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                return NotFound();
            }
            if (!CheckTaskByUser(task, user))
            {
                return Forbid();
            }
            return Ok(task);
        }

        [HttpGet]
        public async Task<IActionResult> GetTicketTasks([FromQuery] int? count, [FromQuery] int? offset,
            [FromQuery] int? statusId, [FromQuery] int? userId)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var user = await db.Users.Include(x => x.Role)
                               .Include(x => x.Departament)
                               .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            var tasks = SortByRole(role, IncludeAllTasks(), user);
            if (statusId != null)
            {
                tasks = tasks.Where(x => x.StatusId == statusId);
            }
            if ((role == "superuser") || (role == "admin"))
            {
                if (userId != null)
                {
                    tasks = tasks.Where(x => x.UserId == userId);
                }
            }
            if (offset != null)
            {
                tasks = tasks.Skip((int)offset);
            }
            if (count != null)
            {
                tasks = tasks.Take((int)count);
            }
            return Ok(tasks);

        }

        [HttpPost]
        public async Task<IActionResult> PostTicketTask([FromBody] TicketTask task)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await db.Users.Include(x => x.Role)
                               .Include(x => x.Departament)
                               .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            var taskUser = db.Users.FirstOrDefault(x => x.Id == task.UserId);
            if (taskUser == null)
            {
                return BadRequest();
            }
            var ticket = await db.Tickets.FirstOrDefaultAsync(x => x.Id == task.TicketId);
            if(ticket == null)
            {
                return BadRequest();
            }
            if ((role == "user") && !((ticket.UserId == user.Id) ||
                ((ticket.UserId == null) && (ticket.DepartamentId == user.DepartamentId))))
            {
                return Forbid();
            }

            if ((role == "superuser") && !((taskUser.DepartamentId == ticket.DepartamentId) || !(ticket.DepartamentId == user.DepartamentId)))
            {
                return Forbid();
            }

            task.DateCreated = DateTime.Now;
            task.StatusId = 1;

            try
            {
                db.TicketTasks.Add(task);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                return BadRequest("Some of the fields are incorrect!");
            }
            catch (Exception e)
            {
                throw e;
            }
            await StaticHelper.RaiseEvent(EventTypes.TaskAdded, task, db);
            return CreatedAtAction("GetTicketTask", new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketTask([FromRoute] int id, PutTicketTask task)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await db.Users.Include(x => x.Role)
                               .Include(x => x.Departament)
                               .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            var taskUser = db.Users.FirstOrDefault(x => x.Id == task.UserId);
            if (task.UserId != null)
            {
                if (taskUser == null)
                {
                    return BadRequest();
                }
            }
            var oldTask = IncludeAllTasks().FirstOrDefault(x => x.Id == id);
            if(oldTask == null)
            {
                return BadRequest();
            }
            if(oldTask.StatusId == 4)
            {
                return BadRequest("Task was complete and can't be modified");
            }

            if(((role == "admin") || (role == "superuser" && oldTask.User.DepartamentId == user.DepartamentId) || 
               ((role == "user") && (oldTask.UserId == user.Id))) && (oldTask.StatusId == 1))
            {
                oldTask.Name = task.Name ?? oldTask.Name;
                oldTask.Content = task.Content ?? oldTask.Content;
                oldTask.UserId = task.UserId ?? oldTask.UserId;
            }
            if ((role == "admin") || (role == "superuser" && oldTask.User.DepartamentId == user.DepartamentId) ||
                (role == "user" && (oldTask.UserId == user.Id)))
            {
                oldTask.StatusId = task.StatusId ?? oldTask.StatusId;
            }
            try
            {
                db.Entry(oldTask).State = EntityState.Modified;

                await db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                return BadRequest("Some of the fields are incorrect!");
            }
            await StaticHelper.RaiseEvent(EventTypes.TaskChanged, oldTask, db);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketTask([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await db.Users.Include(x => x.Role)
                               .Include(x => x.Departament)
                               .FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
            if (user == null)
            {
                return Unauthorized();
            }
            var oldTask = IncludeAllTasks().FirstOrDefault(x => x.Id == id);
            if (oldTask == null)
            {
                return NotFound("Task with provided id wasn't found");
            }
            if(oldTask.StatusId == 1 || role == "admin")
            {
                if ((role == "admin") || ((role == "superuser") && 
                   (user.DepartamentId == oldTask.Ticket.DepartamentId))|| 
                   ((role == "user") && (oldTask.UserId == user.Id)))
                {
                    db.TicketTasks.Remove(oldTask);
                    await db.SaveChangesAsync();
                    return Ok(oldTask);
                }
            }
            return BadRequest("Task is in work, can't delete it");
        }

        private IQueryable<TicketTask> IncludeAllTasks()
        {
            return db.TicketTasks
                .Include(x => x.Status)
                .Include(x => x.Ticket)
                .Include(x => x.User);
        }
        private IQueryable<TicketTask> SortByRole(string role, IQueryable<TicketTask> tasks, User user)
        {
            if(role == "user")
            {
                return tasks.Where(x => x.UserId == user.Id);
            }
            if(role == "superuser")
            {
                return tasks.Where(x => x.User.DepartamentId == user.DepartamentId);
            }
            return tasks;
        }
        private bool CheckTaskByUser(TicketTask task, User user)
        {
            if(((user.Role.Name == "user") && (task.UserId != user.Id)) ||
               ((user.Role.Name == "superuser") && (task.User.DepartamentId != user.DepartamentId)))
            {
                return false;
            }
            return true;
        }
    }
}