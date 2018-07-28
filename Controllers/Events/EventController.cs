using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers.Events
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private AppDbContext db;
        public EventController(AppDbContext _db)
        {
            db = _db;
        }
        [HttpDelete]
        public IActionResult DeleteEvent()
        {
            var count = db.Events.Count();
            db.Database.ExecuteSqlCommandAsync("DELETE FROM Events");
            db.SaveChanges();
            return Ok(count);
        }
    }
}