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
using HelpDesk.Services;

namespace HelpDesk.Controllers.Articles
{
    [Authorize(Roles = "admin,user,superuser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Article
        [Authorize(Roles = "admin,user,superuser,client,superclient")]
        [HttpGet]
        public IActionResult GetArticles([FromQuery] int? sectionId, [FromQuery] int? count, [FromQuery] int? offset, [FromQuery] int? typeId)
        {
            var articles = IncludeAllArticle();
            var role = StaticHelper.GetCurrentRole(User);
            if(role == "client" || role == "superclient")
            {
                typeId = 2;
            }
            if(sectionId != null)
            {
                articles = articles.Where(x => x.SectionId == sectionId);
            }
            if(typeId != null)
            {
                articles = articles.Where(x => x.TypeId == typeId);
            }
            if(offset != null)
            {
                articles = articles.Skip((int)offset);
            }
            if(count != null)
            {
                articles = articles.Take((int)count);
            }
            return new JsonResult(articles);
        }

        // GET: api/Article/5
        [Authorize(Roles = "admin,user,superuser,client,superclient")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await IncludeAllArticle().
                FirstOrDefaultAsync(x => x.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if(role == "client" || role == "superclient")
            {
                if(article.TypeId != 2)
                {
                    return BadRequest();
                }
            }

            return new JsonResult(article);
        }

        // PUT: api/Article/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticle([FromRoute] int id, [FromBody] PutArticle article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var oldArticle = await IncludeAllArticle().FirstOrDefaultAsync(x => x.Id == id);
            if(oldArticle == null)
            {
                return NotFound();
            }
            var role = StaticHelper.GetCurrentRole(User);
            var current = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if(current == null)
            {
                return BadRequest();
            }

            if(role != "admin" && oldArticle.UserId != current.Id)
            {
                return BadRequest();
            }

            oldArticle.Name = article.Name ?? oldArticle.Name;
            oldArticle.Content = article.Content ?? oldArticle.Content;
            oldArticle.SectionId = article.SectionId ?? oldArticle.SectionId;
            oldArticle.TypeId = article.TypeId ?? oldArticle.TypeId;

            _context.Entry(oldArticle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Some values are invalid, can't modify article");
            }

            return NoContent();
        }

        // POST: api/Article
        [HttpPost]
        public async Task<IActionResult> PostArticle([FromBody] Article article)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var current = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (current == null)
            {
                return BadRequest();
            }

            article.UserId = current.Id;
            article.Date = DateTime.Now;

            _context.Articles.Add(article);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Some values are invalid, can't add article");
            }

            return CreatedAtAction("GetArticle", new { id = article.Id }, article);
        }

        // DELETE: api/Article/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = await IncludeAllArticle().FirstOrDefaultAsync(x => x.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            var current = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (current == null)
            {
                return BadRequest();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if(role != "admin" && current.Id != article.UserId)
            {
                return BadRequest();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return Ok(article);
        }
        private IQueryable<Article> IncludeAllArticle()
        {
            return _context.Articles.Include(x => x.Section).Include(x => x.Type);
        }
        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}