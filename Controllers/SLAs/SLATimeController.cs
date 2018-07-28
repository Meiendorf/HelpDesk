using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.SLAs;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.SLAs
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SLATimeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLATimeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SLATime
        [HttpGet]
        public IEnumerable<SLATime> GetSLATime()
        {
            return _context.SLATime.Include(x => x.Sla).Include(x => x.Priority);
        }

        // GET: api/SLATime/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLATime([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLATime = await _context.SLATime
                .Include(x => x.Sla)
                .Include(x => x.Priority)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLATime == null)
            {
                return NotFound();
            }

            return Ok(sLATime);
        }

        // PUT: api/SLATime/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLATime([FromRoute] int id, [FromBody] SLATime sLATime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLATime.Id && sLATime.Id != 0)
            {
                return BadRequest();
            }

            _context.Entry(sLATime).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLATimeExists(id))
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

        // POST: api/SLATime
        [HttpPost]
        public async Task<IActionResult> PostSLATime([FromBody] SLATime sLATime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SLATime.Add(sLATime);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSLATime", new { id = sLATime.Id }, sLATime);
        }

        // DELETE: api/SLATime/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLATime([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLATime = await _context.SLATime
                .Include(x => x.Sla)
                .Include(x => x.Priority)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLATime == null)
            {
                return NotFound();
            }

            _context.SLATime.Remove(sLATime);
            await _context.SaveChangesAsync();

            return Ok(sLATime);
        }

        private bool SLATimeExists(int id)
        {
            return _context.SLATime.Any(e => e.Id == id);
        }
    }
}