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

namespace HelpDesk.Controllers.Tickets
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketAttachmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketAttachmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TicketAttachment
        [HttpGet]
        public IActionResult GetTicketAttachments([FromQuery] int? ticketId)
        {
            if(ticketId == null)
            {
                return BadRequest("Ticket ID is required!");
            }
            var ticket = _context
                .Tickets
                .Include(x => x.Client)
                .ThenInclude(x => x.Company)
                .FirstOrDefault(x => x.Id == ticketId);
            if(ticket == null)
            {
                return NotFound("Ticket with such id doesn't exists!");
            }
            var role = StaticHelper.GetCurrentRole(User);
            if(!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }
            var attachments = IncludeAllAttachments().Where(x => x.TicketId == ticketId);

            return Ok(attachments);
        }

        // GET: api/TicketAttachment/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketAttachment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketAttachment = await IncludeAllAttachments()
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTicketByRole(role, ticketAttachment.Ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            return Ok(ticketAttachment);
        }

        // PUT: api/TicketAttachment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketAttachment()
        {
            return Forbid();
        }

        // POST: api/TicketAttachment
        [HttpPost]
        public async Task<IActionResult> PostTicketAttachment([FromBody] TicketAttachment ticketAttachment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticket = _context.Tickets
                .Include(x => x.Client)
                .ThenInclude(x => x.Company)
                .FirstOrDefault(x => x.Id == ticketAttachment.TicketId);

            if (ticket == null)
            {
                return NotFound("Ticket with such id doesn't exists!");
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }
            try
            {
                ticketAttachment.Type = ticketAttachment.Path.Substring(ticketAttachment.Path.LastIndexOf('.') + 1);
                if(String.IsNullOrWhiteSpace(ticketAttachment.Type))
                {
                    return BadRequest("Unable to define file type.");
                }
            }
            catch
            {
                return BadRequest("Unable to define file type.");
            }
            if(ticketAttachment.Name == null)
            {
                ticketAttachment.Name = "attachment";
            }
            _context.TicketAttachments.Add(ticketAttachment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTicketAttachment", new { id = ticketAttachment.Id }, ticketAttachment);
        }

        // DELETE: api/TicketAttachment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketAttachment([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticketAttachment = await IncludeAllAttachments().FirstOrDefaultAsync(x => x.Id == id);
            if (ticketAttachment == null)
            {
                return NotFound();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (!StaticHelper.CheckTicketByRole(role, ticketAttachment.Ticket, User.Identity.Name, _context))
            {
                return BadRequest();
            }

            _context.TicketAttachments.Remove(ticketAttachment);
            await _context.SaveChangesAsync();

            return Ok(ticketAttachment);
        }
        private IQueryable<TicketAttachment> IncludeAllAttachments()
        {
            return _context.TicketAttachments.Include(x => x.Ticket).ThenInclude(x => x.Client).ThenInclude(x => x.Company);
        }
        private bool TicketAttachmentExists(int id)
        {
            return _context.TicketAttachments.Any(e => e.Id == id);
        }
    }
}