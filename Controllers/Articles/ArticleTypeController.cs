using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Articles;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Articles
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleTypeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ArticleType
        [HttpGet]
        public IEnumerable<ArticleType> GetArticleTypes()
        {
            return _context.ArticleTypes;
        }

        // GET: api/ArticleType/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleType = await _context.ArticleTypes.FindAsync(id);

            if (articleType == null)
            {
                return NotFound();
            }

            return Ok(articleType);
        }

        // PUT: api/ArticleType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticleType([FromRoute] int id, [FromBody] ArticleType articleType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != articleType.Id)
            {
                return BadRequest();
            }

            _context.Entry(articleType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleTypeExists(id))
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

        // POST: api/ArticleType
        [HttpPost]
        public async Task<IActionResult> PostArticleType([FromBody] ArticleType articleType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ArticleTypes.Add(articleType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticleType", new { id = articleType.Id }, articleType);
        }

        // DELETE: api/ArticleType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticleType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleType = await _context.ArticleTypes.FindAsync(id);
            if (articleType == null)
            {
                return NotFound();
            }
            try
            {
                _context.ArticleTypes.Remove(articleType);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete article type because othet entities have reference to it");
            }
            return Ok(articleType);
        }

        private bool ArticleTypeExists(int id)
        {
            return _context.ArticleTypes.Any(e => e.Id == id);
        }
    }
}