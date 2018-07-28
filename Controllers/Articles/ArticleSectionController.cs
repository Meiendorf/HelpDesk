using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Articles;

namespace HelpDesk.Controllers.Articles
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleSectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleSectionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ArticleSection
        [HttpGet]
        public IEnumerable<ArticleSection> GetArticleSections()
        {
            return _context.ArticleSections;
        }

        // GET: api/ArticleSection/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleSection([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleSection = await _context.ArticleSections.FindAsync(id);

            if (articleSection == null)
            {
                return NotFound();
            }

            return Ok(articleSection);
        }

        // PUT: api/ArticleSection/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticleSection([FromRoute] int id, [FromBody] ArticleSection articleSection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != articleSection.Id)
            {
                return BadRequest();
            }

            _context.Entry(articleSection).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleSectionExists(id))
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

        // POST: api/ArticleSection
        [HttpPost]
        public async Task<IActionResult> PostArticleSection([FromBody] ArticleSection articleSection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ArticleSections.Add(articleSection);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticleSection", new { id = articleSection.Id }, articleSection);
        }

        // DELETE: api/ArticleSection/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticleSection([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var articleSection = await _context.ArticleSections.FindAsync(id);
            if (articleSection == null)
            {
                return NotFound();
            }
            try
            {
                _context.ArticleSections.Remove(articleSection);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete article section because other entities have reference to it.");
            }

            return Ok(articleSection);
        }

        private bool ArticleSectionExists(int id)
        {
            return _context.ArticleSections.Any(e => e.Id == id);
        }
    }
}