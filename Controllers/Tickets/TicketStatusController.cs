using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Tickets;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Tickets
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketStatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketStatusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketStatus
        [HttpGet]
        public IEnumerable<TicketStatus> GetTicketStatuses()
        {
            return _context.TicketStatuses;
        }

        // GET: api/TicketStatus/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketStatus = await _context.TicketStatuses.FindAsync(id);

            if (ticketStatus == null)
            {
                return NotFound();
            }

            return Ok(ticketStatus);
        }

        // PUT: api/TicketStatus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketStatus([FromRoute] int id, [FromBody] TicketStatus ticketStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != ticketStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(ticketStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketStatusExists(id))
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

        // POST: api/TicketStatus
        [HttpPost]
        public async Task<IActionResult> PostTicketStatus([FromBody] TicketStatus ticketStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TicketStatuses.Add(ticketStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicketStatus", new { id = ticketStatus.Id }, ticketStatus);
        }

        // DELETE: api/TicketStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketStatus = await _context.TicketStatuses.FindAsync(id);
            if (ticketStatus == null)
            {
                return NotFound();
            }

            try
            {
                _context.TicketStatuses.Remove(ticketStatus);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete ticket status");
            }
            return Ok(ticketStatus);
        }

        private bool TicketStatusExists(int id)
        {
            return _context.TicketStatuses.Any(e => e.Id == id);
        }
    }
}