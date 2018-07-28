using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HelpDesk.Models;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Clients;
using HelpDesk.Models.SLAs;
using HelpDesk.Models.Tickets;
using HelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers.Tickets
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private AppDbContext db { get; set; }

        public TicketController(AppDbContext context)
        {
            db = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var ticket = IncludeAllTicket().FirstOrDefault(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound("No ticket was found!");
            }
            if (!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, db))
            {
                return Forbid();
            }
            await StaticHelper.RaiseEvent(EventTypes.TicketAdded, ticket, db);
            return Ok(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets([FromQuery] int? count, [FromQuery] int? offset, [FromQuery] int? priority, [FromQuery] int? status)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var tickets = SortByRole(role, IncludeAllTicket());
            if(tickets == null)
            {
                return BadRequest("No tickets was found for your account");
            }
            if(priority != null)
            {
                tickets = tickets.Where(x => x.PriorityId == priority);
            }
            if (status != null)
            {
                tickets = tickets.Where(x => x.StatusId == status);
            }
            if (offset != null)
            {
                tickets = tickets.Skip((int)offset);
            }
            if (count != null)
            {
                tickets = tickets.Take((int)count);
            }
            
            return Ok(tickets);
            
        }
        
        [HttpPost]
        public async Task<IActionResult> PostTicket([FromBody] Ticket ticket)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(!StaticHelper.CheckTicketByRole(role, ticket, User.Identity.Name, db))
            {
                return Forbid();
            }
            var slaId = db.Companies.FirstOrDefault(x => x.Id == db.Clients.
                FirstOrDefault(t => t.Id == ticket.ClientId).CompanyId).SLAId;

            if (!CheckTicketBySLA(ticket, slaId))
            {
                return BadRequest("Some values are not allowed by SLA, associated with company of client");
            }

            ticket.DateModified = DateTime.Now;
            ticket.DateCreated = DateTime.Now;
            ticket.StatusId = 5;
            
            try
            {
                db.Tickets.Add(ticket);
                await db.SaveChangesAsync();
            }
            catch(DbUpdateException e)
            {
                return BadRequest("Some of the fields are incorrect!");
            }
            catch(Exception e)
            {
                throw e;
            }

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicket([FromRoute] int id,[FromBody]  PutTicket ticket)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var oldTicket = IncludeAllTicket().FirstOrDefault(x => x.Id == id);
            if(oldTicket == null)
            {
                return BadRequest();
            }
            var email = User.Identity.Name;
            AppUser user;
            var companyId = 0;
            if(role == "client" || role == "superclient")
            {
                if(oldTicket.StatusId != 5)
                {
                    return BadRequest("Ticket is in work, can't change it!");
                }
                user = db.Clients.FirstOrDefault(x => x.Email == email);
            }
            else
            {
                user = db.Users.FirstOrDefault(x => x.Email == email);
            }
            if((role == "admin") || 
               ((role == "client" && oldTicket.ClientId == user.Id) ||
               (role == "superclient" && oldTicket.Client.CompanyId == (user as Client).CompanyId)))
            {
                oldTicket.Name = ticket.Name ?? oldTicket.Name;
                oldTicket.Content = ticket.Content ?? oldTicket.Content;
                oldTicket.TypeId = ticket.TypeId ?? oldTicket.TypeId;
                oldTicket.PriorityId = ticket.PriorityId ?? oldTicket.PriorityId;
                oldTicket.DepartamentId = ticket.DepartamentId ?? oldTicket.DepartamentId;
                oldTicket.UserId = ticket.UserId ?? oldTicket.UserId;
                oldTicket.ObjectiveId = ticket.ObjectiveId ?? oldTicket.ObjectiveId;
            }
            
            var statusId = ticket.StatusId;
            if(statusId != null)
            {
                if(role != "admin" && ((statusId == 1) || (statusId == 2)))
                {
                    statusId = null;
                }
                if ((role == "client" || role == "superclient") && (statusId != 5))
                {
                    statusId = null;
                }
            }

            oldTicket.StatusId = statusId ?? oldTicket.StatusId;

            oldTicket.DateModified = DateTime.Now;

            if(!StaticHelper.CheckTicketByRole(role, oldTicket, User.Identity.Name, db))
            {
                return BadRequest();
            }
            var slaId = db.Companies.FirstOrDefault(x => x.Id == db.Clients.
                FirstOrDefault(t => t.Id == oldTicket.ClientId).CompanyId).SLAId;

            if (!CheckTicketBySLA(oldTicket, slaId))
            {
                return BadRequest("Some values are not allowed by SLA, associated with company of client");
            }
            db.Entry(oldTicket).State = EntityState.Modified;
 
            await db.SaveChangesAsync();
           
            return NoContent();
        }

        [Authorize(Roles = "admin,client,superclient")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ticket = await IncludeAllTicket().FirstOrDefaultAsync(x => x.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }
            var email = User.Identity.Name;
            AppUser user;
            if (role == "client" || role == "superclient")
            {
                if (ticket.StatusId != 5)
                {
                    return BadRequest("Ticket is in work, can't delete it!");
                }
                user = db.Clients.FirstOrDefault(x => x.Email == email);
            }
            else
            {
                user = db.Users.FirstOrDefault(x => x.Email == email);
            }
            if ((role == "admin") ||
               ((role == "client" && ticket.ClientId == user.Id) ||
                (role == "superclient" && ticket.Client.CompanyId == (user as Client).CompanyId)))
            {
                db.Tickets.Remove(ticket);
                await db.SaveChangesAsync();
                return Ok(ticket);
            }
            else
            {
                return Forbid();
            }      
        }

        private IQueryable<Ticket> SortByRole(string role, IQueryable<Ticket> tickets)
        {
            if (role == "client")
            {
                var client = db.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (client == null)
                {
                    return null;
                }
                tickets = tickets.Where(x => x.ClientId == client.Id);
            }
            else if (role == "superclient")
            {
                var client = db.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (client == null)
                {
                    return null;
                }
                tickets = tickets.Where(x => x.Client.CompanyId == client.CompanyId);
            }
            else if (role == "user")
            {
                var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (user == null)
                {
                    return null;
                }
                tickets = tickets.Where(x => ((x.UserId == user.Id) ||
                    ((x.DepartamentId == user.DepartamentId) && (x.UserId == null))));
            }
            else if (role == "superuser")
            {
                var user = db.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (user == null)
                {
                    return null;
                }
                tickets = tickets.Where(x => x.DepartamentId == user.DepartamentId);
            }
            else if(role != "admin")
            {
                return null;
            }
            return tickets;
        }
        private IQueryable<Ticket> IncludeAllTicket()
        {
            return db.Tickets
                .Include(x => x.Client).ThenInclude(x => x.Company)
                .Include(x => x.Priority)
                .Include(x => x.User)
                .Include(x => x.Type)
                .Include(x => x.Status)
                .Include(x => x.Objective)
                .Include(x => x.Departament)
                .Include(x => x.UserInit);
        }
        
        private bool CheckTicketBySLA(Ticket ticket, int SlaId)
        {
            try
            {
                var company = db.Companies.FirstOrDefault
                    (x => x.Id == db.Clients.FirstOrDefault(t => t.Id == ticket.ClientId).CompanyId);
                if (company.SLAId != SlaId)
                {
                    return false;
                }
                if ((ticket.DepartamentId != null) && (db.SLAAllowedDepartaments.Where(x => x.SLAId == SlaId)
                    .FirstOrDefault(x => x.DepartamentId == ticket.DepartamentId) == null))
                {
                    return false;
                }
                if (ticket.ObjectiveId != null)
                {
                    if (db.SLAAllowedObjectives.Where(x => x.SLAId == SlaId).
                        FirstOrDefault(x => x.ObjectiveId == ticket.ObjectiveId) == null)
                    {
                        return false;
                    }
                }
                if (db.SLAAllowedPriorities.Where(x => x.SLAId == SlaId).
                    FirstOrDefault(x => x.PriorityId == ticket.PriorityId) == null)
                {
                    return false;
                }
                if (db.SLAAllowedTypes.Where(x => x.SLAId == SlaId).
                    FirstOrDefault(x => x.TypeId == ticket.TypeId) == null)
                {
                    return false;
                }
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}