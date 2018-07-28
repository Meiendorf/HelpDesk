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
using HelpDesk.Models.SLAs.ControllerModels;
using Newtonsoft.Json;

namespace HelpDesk.Controllers.SLAs
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SLADepartamentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLADepartamentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<SLAAllowedDepartament> GetSLAAllowedDepartaments()
        {
            return _context.SLAAllowedDepartaments.Include(x => x.Departament).Include(x => x.Sla);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLAAllowedDepartament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedDepartament = await _context.SLAAllowedDepartaments
                .Include(x => x.Departament)
                .Include(x => x.Sla)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedDepartament == null)
            {
                return NotFound();
            }

            return Ok(sLAAllowedDepartament);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLAAllowedDepartament([FromRoute] int id, [FromBody] SLAAllowedDepartament sLAAllowedDepartament)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLAAllowedDepartament.Id && sLAAllowedDepartament.Id != 0)
            {
                return BadRequest();
            }

            _context.Entry(sLAAllowedDepartament).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLAAllowedDepartamentExists(id))
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
        public async Task<IActionResult> PostSLAAllowedDepartament([FromBody] SLAPostDepartaments departaments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var count = 0;
            var skipped = new List<SLAAllowedDepartament>();
            foreach (var element in departaments.Departaments)
            {
                element.Unique = element.SLAId.ToString() + "_" + element.DepartamentId.ToString();
                _context.SLAAllowedDepartaments.Add(element);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLAAllowedDepartament([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedDepartament = await _context.SLAAllowedDepartaments
                .Include(x => x.Departament)
                .Include(x => x.Sla)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedDepartament == null)
            {
                return NotFound();
            }

            _context.SLAAllowedDepartaments.Remove(sLAAllowedDepartament);
            await _context.SaveChangesAsync();

            return Ok(sLAAllowedDepartament);
        }

        private bool SLAAllowedDepartamentExists(int id)
        {
            return _context.SLAAllowedDepartaments.Any(e => e.Id == id);
        }
    }
}