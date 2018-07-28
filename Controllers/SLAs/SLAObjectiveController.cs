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
    public class SLAObjectiveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLAObjectiveController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<SLAAllowedObjective> GetSLAAllowedObjectives()
        {
            return _context.SLAAllowedObjectives.Include(x => x.Sla).Include(x => x.Objective);
        }

        // GET: api/SLAAObjective/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLAAllowedObjective([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedObjective = await _context.SLAAllowedObjectives
                .Include(x => x.Sla)
                .Include(x => x.Objective)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedObjective == null)
            {
                return NotFound();
            }

            return Ok(sLAAllowedObjective);
        }

        // PUT: api/SLAAObjective/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLAAllowedObjective([FromRoute] int id, [FromBody] SLAAllowedObjective sLAAllowedObjective)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLAAllowedObjective.Id && sLAAllowedObjective.Id != 0)
            {
                return BadRequest();
            }

            _context.Entry(sLAAllowedObjective).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLAAllowedObjectiveExists(id))
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

        // POST: api/SLAAObjective
        [HttpPost]
        public async Task<IActionResult> PostSLAAllowedObjective([FromBody] SLAPostObjectives objectives)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var count = 0;
            var skipped = new List<SLAAllowedObjective>();
            foreach (var element in objectives.Objectives)
            {
                element.Unique = element.SLAId.ToString() + "_" + element.ObjectiveId.ToString();
                _context.SLAAllowedObjectives.Add(element);
                try
                {
                    await _context.SaveChangesAsync();
                    count++;
                }
                catch (Exception e)
                {
                    _context.Remove(element);
                    element.Id = 0;
                    skipped.Add(element);
                }
            }

            return Ok($"{count} rows was added. Invalid values :\n" + JsonConvert.SerializeObject(skipped));
        }

        // DELETE: api/SLAAObjective/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLAAllowedObjective([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedObjective = await _context.SLAAllowedObjectives
                .Include(x => x.Sla)
                .Include(x => x.Objective)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedObjective == null)
            {
                return NotFound();
            }

            _context.SLAAllowedObjectives.Remove(sLAAllowedObjective);
            await _context.SaveChangesAsync();

            return Ok(sLAAllowedObjective);
        }

        private bool SLAAllowedObjectiveExists(int id)
        {
            return _context.SLAAllowedObjectives.Any(e => e.Id == id);
        }
    }
}