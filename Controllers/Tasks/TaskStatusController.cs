using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Task;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Tasks
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TaskStatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskStatusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TaskStatus
        [HttpGet]
        public IEnumerable<TicketTaskStatus> GetTaskStatuses()
        {
            return _context.TaskStatuses;
        }

        // GET: api/TaskStatus/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketTaskStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketTaskStatus = await _context.TaskStatuses.FindAsync(id);

            if (ticketTaskStatus == null)
            {
                return NotFound();
            }

            return Ok(ticketTaskStatus);
        }

        // PUT: api/TaskStatus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketTaskStatus([FromRoute] int id, [FromBody] TicketTaskStatus ticketTaskStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != ticketTaskStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(ticketTaskStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketTaskStatusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/TaskStatus
        [HttpPost]
        public async Task<IActionResult> PostTicketTaskStatus([FromBody] TicketTaskStatus ticketTaskStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TaskStatuses.Add(ticketTaskStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicketTaskStatus", new { id = ticketTaskStatus.Id }, ticketTaskStatus);
        }

        // DELETE: api/TaskStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketTaskStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketTaskStatus = await _context.TaskStatuses.FindAsync(id);
            if (ticketTaskStatus == null)
            {
                return NotFound();
            }
            try
            {
                _context.TaskStatuses.Remove(ticketTaskStatus);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete task status");
            }

            return Ok(ticketTaskStatus);
        }

        private bool TicketTaskStatusExists(int id)
        {
            return _context.TaskStatuses.Any(e => e.Id == id);
        }
    }
}