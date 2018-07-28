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
    public class SLAController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLAController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<SLA> GetSLAs()
        {
            return _context.SLAs;
        }

       
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLA([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLA = await _context.SLAs.FindAsync(id);

            if (sLA == null)
            {
                return NotFound();
            }

            return Ok(sLA);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLA([FromRoute] int id, [FromBody] SLA sLA)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLA.Id && sLA.Id != 0)
            {
                return BadRequest("SLA id wasn't found in request body, or provided IDs don't match");
            }

            _context.Entry(sLA).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLAExists(id))
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

        [HttpPost]
        public async Task<IActionResult> PostSLA([FromBody] SLA sLA)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SLAs.Add(sLA);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSLA", new { id = sLA.Id }, sLA);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLA([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLA = await _context.SLAs.FindAsync(id);
            if (sLA == null)
            {
                return NotFound();
            }

            _context.SLAs.Remove(sLA);
            await _context.SaveChangesAsync();

            return Ok(sLA);
        }

        private bool SLAExists(int id)
        {
            return _context.SLAs.Any(e => e.Id == id);
        }
    }
}