using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Notifications;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Events
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EventTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EventType
        [HttpGet]
        public IEnumerable<EventType> GetEventTypes()
        {
            return _context.EventTypes;
        }

        // GET: api/EventType/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventType = await _context.EventTypes.FindAsync(id);

            if (eventType == null)
            {
                return NotFound();
            }

            return Ok(eventType);
        }

        // PUT: api/EventType/5
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEventType([FromRoute] int id, [FromBody] EventType eventType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != eventType.Id)
            {
                return BadRequest();
            }

            _context.Entry(eventType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventTypeExists(id))
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

        // POST: api/EventType
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PostEventType([FromBody] EventType eventType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.EventTypes.Add(eventType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEventType", new { id = eventType.Id }, eventType);
        }

        // DELETE: api/EventType/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteEventType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null)
            {
                return NotFound();
            }
            try
            {
                _context.EventTypes.Remove(eventType);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete event type because other entities have reference to it");
            }
            return Ok(eventType);
        }

        private bool EventTypeExists(int id)
        {
            return _context.EventTypes.Any(e => e.Id == id);
        }
    }
}