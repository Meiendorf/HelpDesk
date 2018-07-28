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
    public class TicketTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketType
        [HttpGet]
        public IEnumerable<TicketType> GetTicketTypes()
        {
            return _context.TicketTypes;
        }

        // GET: api/TicketType/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketType = await _context.TicketTypes.FindAsync(id);

            if (ticketType == null)
            {
                return NotFound();
            }

            return Ok(ticketType);
        }

        // PUT: api/TicketType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketType([FromRoute] int id, [FromBody] TicketType ticketType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != ticketType.Id)
            {
                return BadRequest();
            }

            _context.Entry(ticketType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketTypeExists(id))
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

        // POST: api/TicketType
        [HttpPost]
        public async Task<IActionResult> PostTicketType([FromBody] TicketType ticketType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TicketTypes.Add(ticketType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicketType", new { id = ticketType.Id }, ticketType);
        }

        // DELETE: api/TicketType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketType = await _context.TicketTypes.FindAsync(id);
            if (ticketType == null)
            {
                return NotFound();
            }
            try
            {
                _context.TicketTypes.Remove(ticketType);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete ticket type");
            }

            return Ok(ticketType);
        }

        private bool TicketTypeExists(int id)
        {
            return _context.TicketTypes.Any(e => e.Id == id);
        }
    }
}