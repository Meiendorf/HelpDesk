using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.Services;
using System.Security.Claims;
using Newtonsoft.Json;

namespace HelpDesk.Controllers.Events
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Notification
        [HttpGet]
        public IActionResult GetNotifications()
        {
            var notifications = _context.Notifications.Include(x => x.EventType) as IQueryable<Notification>;

            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                if (role == "client" || role == "superclient")
                {
                    var client = _context.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                    if (client == null)
                    {
                        return Unauthorized();
                    }
                    notifications = notifications.Where(x => x.ClientId == client.Id);
                }
                else
                {
                    var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                    if (user == null)
                    {
                        return Unauthorized();
                    }
                    notifications = notifications.Where(x => x.UserId == user.Id);
                }
            }
            return Ok(JsonConvert.SerializeObject(notifications.ToArray()));
        }

        // GET: api/Notification/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var notification = await _context.Notifications
                .Include(x => x.EventType)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (notification == null)
            {
                return NotFound();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                var normal = await CheckNotification(notification, User);
                if (!normal)
                {
                    return BadRequest();
                }
            }
            return Ok(notification);
        }

        // PUT: api/Notification/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNotification([FromRoute] int id, [FromBody] Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != notification.Id)
            {
                return BadRequest();
            }
            var role = StaticHelper.GetCurrentRole(User);
            if (role != "admin")
            {
                var normal = await CheckNotification(notification, User);
                if (!normal)
                {
                    return BadRequest();
                }
            }

            _context.Entry(notification).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NotificationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest("Some values are invalid!");
                }
            }

            return NoContent();
        }

        private async Task<bool> CheckNotification(Notification notification, ClaimsPrincipal User)
        {
            var role = StaticHelper.GetCurrentRole(User);
            if (role == "client" || role == "superclient")
            {
                if(notification.EventTypeId > 3)
                {
                    return false;
                }
                var client = _context.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (client == null)
                {
                    return false;
                }
                if (notification.ClientId != client.Id)
                {
                    return false;
                }
                notification.UserId = null;
            }
            else
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (user == null)
                {
                    return false;
                }
                if (user.Id != notification.UserId)
                {
                    return false;
                }
                notification.ClientId = null;
            }
            return true;
        }
        // POST: api/Notification
        [HttpPost]
        public async Task<IActionResult> PostNotification([FromBody] Notification notification)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = StaticHelper.GetCurrentRole(User);
            if (role == "client" || role == "superclient")
            {
                var client = _context.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (role != "admin")
                {
                    if(notification.EventTypeId > 3)
                    {
                        return BadRequest();
                    }
                    if (notification.ClientId != client.Id)
                    {
                        return BadRequest();
                    }
                }
                var exist = _context.Notifications.FirstOrDefault(x => x.ClientId == client.Id
                    && x.EventTypeId == notification.EventTypeId);
                if (exist != null)
                {
                    return BadRequest("On this account already exists subscription to this event;");
                }
                notification.UserId = null;
            }
            else
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                if (role != "admin")
                {
                    if (user.Id != notification.UserId)
                    {
                        return BadRequest();
                    }
                }
                var exist = _context.Notifications.FirstOrDefault(x => x.UserId == user.Id 
                    && x.EventTypeId == notification.EventTypeId);
                if(exist != null)
                {
                    return BadRequest("On this account already exists subscription to this event;");
                }
                notification.UserId = null;
            }
 
            try
            {
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Some values are invalid or duplicate existing values");
            }

            return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
        }

        // DELETE: api/Notification/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var notification = await _context.Notifications
                .Include(x => x.EventType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (notification == null)
            {
                return NotFound();
            }
            if (StaticHelper.GetCurrentRole(User) != "admin")
            {
                var normal = await CheckNotification(notification, User);
                if (!normal)
                {
                    return BadRequest();
                }
            }
            
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(notification);
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }
    }
}