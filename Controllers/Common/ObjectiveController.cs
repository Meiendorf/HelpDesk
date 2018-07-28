using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Common;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Common
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectiveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ObjectiveController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Objective
        [HttpGet]
        public IEnumerable<Objective> GetObjectives()
        {
            return _context.Objectives;
        }

        // GET: api/Objective/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetObjective([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var objective = await _context.Objectives.FindAsync(id);

            if (objective == null)
            {
                return NotFound();
            }

            return Ok(objective);
        }

        // PUT: api/Objective/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutObjective([FromRoute] int id, [FromBody] Objective objective)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != objective.Id)
            {
                return BadRequest();
            }

            _context.Entry(objective).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectiveExists(id))
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

        // POST: api/Objective
        [HttpPost]
        public async Task<IActionResult> PostObjective([FromBody] Objective objective)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Objectives.Add(objective);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetObjective", new { id = objective.Id }, objective);
        }

        // DELETE: api/Objective/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObjective([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var objective = await _context.Objectives.FindAsync(id);
            if (objective == null)
            {
                return NotFound();
            }

            try
            {
                _context.Objectives.Remove(objective);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete objective");
            }

            return Ok(objective);
        }

        private bool ObjectiveExists(int id)
        {
            return _context.Objectives.Any(e => e.Id == id);
        }
    }
}