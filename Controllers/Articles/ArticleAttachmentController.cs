using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Tickets;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.Services;
using HelpDesk.Models.Articles;

namespace HelpDesk.Controllers.Articles
{
    [Authorize(Roles = "admin,user,superuser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleAttachmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleAttachmentController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "admin,user,superuser,client,superclient")]
        [HttpGet]
        public IActionResult GetArticleAttachments([FromQuery] int? articleId)
        {
            if (articleId == null)
            {
                return BadRequest("Article ID is required!");
            }

            var article = _context
                .Articles
                .FirstOrDefault(x => x.Id == articleId);

            if (article == null)
            {
                return NotFound("Article with such id doesn't exists!");
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (role == "client" && role == "superclient")
            {
                if (article.TypeId != 2)
                {
                    return BadRequest();
                }
            }
            var attachments = IncludeAllAttachments().Where(x => x.ArticleId == articleId);

            return new JsonResult(attachments);
        }

        [Authorize(Roles = "admin,user,superuser,client,superclient")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticleAttachment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleAttachment = await IncludeAllAttachments()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (articleAttachment == null)
            {
                return NotFound();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if(role == "client" || role == "superclient")
            {
                if(articleAttachment.Article.TypeId != 2)
                {
                    return BadRequest();
                }
            }
            
            return new JsonResult(articleAttachment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutArticleAttachment()
        {
            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> PostArticleAttachment([FromBody] ArticleAttachment articleAttachment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var article = _context
                .Articles
                .FirstOrDefault(x => x.Id == articleAttachment.ArticleId);

            if (article == null)
            {
                return NotFound("Article with such id doesn't exists!");
            }
            var role = StaticHelper.GetCurrentRole(User);
            var current = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (current == null)
            {
                return BadRequest();
            }
            if (role != "admin" & article.UserId != current.Id)
            {
                return BadRequest();
            }
            try
            {
                articleAttachment.Type = articleAttachment.Path.Substring(articleAttachment.Path.LastIndexOf('.') + 1);
                if (String.IsNullOrWhiteSpace(articleAttachment.Type))
                {
                    return BadRequest("Unable to define file type.");
                }
            }
            catch
            {
                return BadRequest("Unable to define file type.");
            }

            if (articleAttachment.Name == null)
            {
                articleAttachment.Name = "attachment";
            }
            _context.ArticleAttachments.Add(articleAttachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArticleAttachment", new { id = articleAttachment.Id }, articleAttachment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticleAttachment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var articleAttachment = await IncludeAllAttachments().FirstOrDefaultAsync(x => x.Id == id);
            if (articleAttachment == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            var current = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
            if (current == null)
            {
                return BadRequest();
            }
            if (role != "admin" & articleAttachment.Article.UserId != current.Id)
            {
                return BadRequest();
            }

            _context.ArticleAttachments.Remove(articleAttachment);
            await _context.SaveChangesAsync();

            return new JsonResult(articleAttachment);
        }
        private IQueryable<ArticleAttachment> IncludeAllAttachments()
        {
            return _context.ArticleAttachments.Include(x => x.Article).ThenInclude(x => x.User);
        }
    }
}