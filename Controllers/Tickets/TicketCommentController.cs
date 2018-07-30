using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Tickets;
using HelpDesk.Services;
using HelpDesk.Models.Users;
using HelpDesk.Models.Clients;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Tickets
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketCommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketCommentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketComment
        [HttpGet]
        public IActionResult GetTicketComments([FromQuery] int? ticketId)
        {
            if (ticketId == null)
            {
                return BadRequest("Ticket ID is required!");
            }

            var ticket = _context
                .Tickets
                .Include(x => x.Client)
                .ThenInclude(x => x.Company)
                .FirstOrDefault(x => x.Id == ticketId);

            if (ticket == null)
            {
                return NotFound("Ticket with such id doesn't exists!");
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            var serializerSettings = new JsonSerializerSettings();
            
            if (role == "client" || role == "superclient")
            {
                var resolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                resolver.IgnoreProperty(typeof(User), "Email", "Phone");
                resolver.IgnoreProperty(typeof(Client), "Email", "Phone");
                serializerSettings.ContractResolver = resolver;
            }
            var ticketComments = IncludeAllComments().Where(x => x.TicketId == ticketId);

            return Ok(JsonConvert.SerializeObject(ticketComments, serializerSettings));
        }

        // GET: api/TicketComment/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (ticketComment == null)
            {
                return NotFound();
            }

            var role = StaticHelper.GetCurrentRole(User);
            if(!StaticHelper.CheckTicketByRole(role, ticketComment.Ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            var serializerSettings = new JsonSerializerSettings();

            if (role == "client" || role == "superclient")
            {
                var resolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                resolver.IgnoreProperty(typeof(User), "Email", "Phone");
                resolver.IgnoreProperty(typeof(Client), "Email", "Phone");
                serializerSettings.ContractResolver = resolver;
            }

            return Ok(JsonConvert.SerializeObject(ticketComment, serializerSettings));
        }

        // PUT: api/TicketComment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketComment([FromRoute] int id, [FromQuery] string content)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(content == null)
            {
                return BadRequest("Content is required!");
            }
            var ticketComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (ticketComment == null)
            {
                return NotFound();
            }
            if(ticketComment.Ticket == null)
            {
                return BadRequest();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                if(role == "client" || role == "superclient")
                {
                    if(ticketComment.Client.Email != User.Identity.Name || ticketComment.Client == null)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    if (ticketComment.User.Email != User.Identity.Name || ticketComment.User == null)
                    {
                        return BadRequest();
                    }
                }
            }

            ticketComment.Content = content;

            _context.Entry(ticketComment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketCommentExists(id))
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

        // POST: api/TicketComment
        [HttpPost]
        public async Task<IActionResult> PostTicketComment([FromBody] TicketComment ticketComment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = StaticHelper.GetCurrentRole(User);
            var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.Id == ticketComment.TicketId);
            if(ticket == null)
            {
                return BadRequest();
            }
            if (!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            if (role == "client" || role == "superclient")
            {
                var client = await _context.Clients.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
                if(client == null)
                {
                    return BadRequest();
                }
                ticketComment.ClientId = client.Id;
                ticketComment.UserId = null;
            }
            else
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
                if (user == null)
                {
                    return BadRequest();
                }
                ticketComment.UserId = user.Id;
                ticketComment.ClientId = null;
            }

            _context.TicketComments.Add(ticketComment);
            await _context.SaveChangesAsync();
            await StaticHelper.RaiseEvent(EventTypes.TicketComment, ticket, _context);
            return CreatedAtAction("GetTicketComment", new { id = ticketComment.Id }, ticketComment);
        }

        // DELETE: api/TicketComment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketComment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketComment = await IncludeAllComments().FirstOrDefaultAsync(x => x.Id == id);

            if (ticketComment == null)
            {
                return NotFound();
            }
            if (ticketComment.Ticket == null)
            {
                return BadRequest();
            }

            var role = StaticHelper.GetCurrentRole(User);
            var serializerSettings = new JsonSerializerSettings();
            if (role != "admin")
            {
                if (role == "client" || role == "superclient")
                {
                    var resolver = new PropertyRenameAndIgnoreSerializerContractResolver();
                    resolver.IgnoreProperty(typeof(User), "Email", "Phone");
                    resolver.IgnoreProperty(typeof(Client), "Email", "Phone");
                    serializerSettings.ContractResolver = resolver;
                    
                    if (ticketComment.Client.Email != User.Identity.Name || ticketComment.Client == null)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    if (ticketComment.User.Email != User.Identity.Name || ticketComment.User == null)
                    {
                        return BadRequest();
                    }
                }
            }

            _context.TicketComments.Remove(ticketComment);
            await _context.SaveChangesAsync();

            return Ok(JsonConvert.SerializeObject(ticketComment, serializerSettings));
        }
        private IQueryable<TicketComment> IncludeAllComments()
        {
            return _context.TicketComments.Include(x => x.User).Include(x => x.Client).Include(x => x.Ticket);
        }
        private bool TicketCommentExists(int id)
        {
            return _context.TicketComments.Any(e => e.Id == id);
        }
    }
}