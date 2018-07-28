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
    public class SLATypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SLATypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/SLAType
        [HttpGet]
        public IEnumerable<SLAAllowedType> GetSLAAllowedTypes()
        {
            return _context.SLAAllowedTypes.Include(x => x.Sla).Include(x => x.Type);
        }

        // GET: api/SLAType/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSLAAllowedType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedType = await _context.SLAAllowedTypes
                .Include(x => x.Sla)
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedType == null)
            {
                return NotFound();
            }

            return Ok(sLAAllowedType);
        }

        // PUT: api/SLAType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSLAAllowedType([FromRoute] int id, [FromBody] SLAAllowedType sLAAllowedType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sLAAllowedType.Id && sLAAllowedType.Id != 0)
            {
                return BadRequest();
            }

            _context.Entry(sLAAllowedType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SLAAllowedTypeExists(id))
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

        // POST: api/SLAType
        [HttpPost]
        public async Task<IActionResult> PostSLAAllowedType([FromBody] SLAPostTypes types)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var count = 0;
            var skipped = new List<SLAAllowedType>();
            foreach (var element in types.Types)
            {
                element.Unique = element.SLAId.ToString() + "_" + element.TypeId.ToString();
                _context.SLAAllowedTypes.Add(element);
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

        // DELETE: api/SLAType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSLAAllowedType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sLAAllowedType = await _context.SLAAllowedTypes
                .Include(x => x.Sla)
                .Include(x => x.Type)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sLAAllowedType == null)
            {
                return NotFound();
            }

            _context.SLAAllowedTypes.Remove(sLAAllowedType);
            await _context.SaveChangesAsync();

            return Ok(sLAAllowedType);
        }

        private bool SLAAllowedTypeExists(int id)
        {
            return _context.SLAAllowedTypes.Any(e => e.Id == id);
        }
    }
}