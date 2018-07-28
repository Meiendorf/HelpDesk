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

namespace HelpDesk.Controllers.Users
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DepartamentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Departament
        [HttpGet]
        public IEnumerable<Departament> GetDepartaments()
        {
            return _context.Departaments;
        }

        // GET: api/Departament/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var departament = await _context.Departaments.FindAsync(id);

            if (departament == null)
            {
                return NotFound();
            }

            return Ok(departament);
        }

        // PUT: api/Departament/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDepartament([FromRoute] int id, [FromBody] Departament departament)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != departament.Id)
            {
                return BadRequest();
            }

            _context.Entry(departament).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartamentExists(id))
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

        // POST: api/Departament
        [HttpPost]
        public async Task<IActionResult> PostDepartament([FromBody] Departament departament)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Departaments.Add(departament);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDepartament", new { id = departament.Id }, departament);
        }

        // DELETE: api/Departament/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var departament = await _context.Departaments.FindAsync(id);
            if (departament == null)
            {
                return NotFound();
            }
            try
            {
                _context.Departaments.Remove(departament);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete departament because other entities have reference to it.");
            }

            return Ok(departament);
        }

        private bool DepartamentExists(int id)
        {
            return _context.Departaments.Any(e => e.Id == id);
        }
    }
}