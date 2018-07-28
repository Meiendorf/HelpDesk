using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.SLAs;
using HelpDesk.Models.SLAs.ControllerModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.SLAs
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SLAPriorityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLAPriorityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SLAPriority
        [HttpGet]
        public IEnumerable<SLAAllowedPriority> GetSLAAllowedPriorities()
        {
            return _context.SLAAllowedPriorities.Include(x => x.Sla).Include(x => x.Priority);
        }

        // GET: api/SLAPriority/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLAAllowedPriority([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedPriority = await _context.SLAAllowedPriorities
                .Include(x => x.Sla)
                .Include(x => x.Priority)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedPriority == null)
            {
                return NotFound();
            }

            return Ok(sLAAllowedPriority);
        }

        // PUT: api/SLAPriority/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLAAllowedPriority([FromRoute] int id, [FromBody] SLAAllowedPriority sLAAllowedPriority)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLAAllowedPriority.Id && sLAAllowedPriority.Id != 0)
            {
                return BadRequest();
            }

            _context.Entry(sLAAllowedPriority).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLAAllowedPriorityExists(id))
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

        // POST: api/SLAPriority
        [HttpPost]
        public async Task<IActionResult> PostSLAAllowedPriority([FromBody] SLAPostPriorities sLAAllowedPriority)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var count = 0;
            var skipped = new List<SLAAllowedPriority>();
            foreach(var priority in sLAAllowedPriority.Priorities)
            {
                priority.Unique = priority.SLAId.ToString() + "_" + priority.PriorityId.ToString();
                _context.SLAAllowedPriorities.Add(priority);
                try
                {
                    await _context.SaveChangesAsync();
                    count++;
                }
                catch(Exception e)
                {
                    _context.Remove(priority);
                    priority.Id = 0;
                    skipped.Add(priority);
                }
            }

            return Ok($"{count} rows was added. Invalid values :\n" + JsonConvert.SerializeObject(skipped));
        }

        // DELETE: api/SLAPriority/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLAAllowedPriority([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedPriority = await _context.SLAAllowedPriorities
                .Include(x => x.Sla)
                .Include(x => x.Priority)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedPriority == null)
            {
                return NotFound();
            }

            _context.SLAAllowedPriorities.Remove(sLAAllowedPriority);
            await _context.SaveChangesAsync();

            return Ok(sLAAllowedPriority);
        }

        private bool SLAAllowedPriorityExists(int id)
        {
            return _context.SLAAllowedPriorities.Any(e => e.Id == id);
        }
    }
}