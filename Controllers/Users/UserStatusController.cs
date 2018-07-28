using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Users;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Users
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserStatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserStatusController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/UserStatus
        [HttpGet]
        public IEnumerable<UserStatus> GetUserStatuses()
        {
            return _context.UserStatuses;
        }

        // GET: api/UserStatus/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userStatus = await _context.UserStatuses.FindAsync(id);

            if (userStatus == null)
            {
                return NotFound();
            }

            return Ok(userStatus);
        }

        // PUT: api/UserStatus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserStatus([FromRoute] int id, [FromBody] UserStatus userStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != userStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(userStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserStatusExists(id))
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

        // POST: api/UserStatus
        [HttpPost]
        public async Task<IActionResult> PostUserStatus([FromBody] UserStatus userStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.UserStatuses.Add(userStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserStatus", new { id = userStatus.Id }, userStatus);
        }

        // DELETE: api/UserStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserStatus([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userStatus = await _context.UserStatuses.FindAsync(id);
            if (userStatus == null)
            {
                return NotFound();
            }
            try
            {
                _context.UserStatuses.Remove(userStatus);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete user status");
            }
            return Ok(userStatus);
        }

        private bool UserStatusExists(int id)
        {
            return _context.UserStatuses.Any(e => e.Id == id);
        }
    }
}