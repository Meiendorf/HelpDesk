using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Users;
using HelpDesk.Services;
using Microsoft.AspNetCore.Identity;
using HelpDesk.Models;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.Controllers.Users
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/User
        [Authorize(Roles = "admin,superuser")]
        [HttpGet]
        public IActionResult GetUsers()
        {
            var role = StaticHelper.GetCurrentRole(User);
            var users = _context.Users
                .Include(x => x.Departament)
                .Include(x => x.Status)
                .Include(x => x.Role) as IQueryable<User>;
            if (role == "superuser")
            {
                var user = _context.Users.FirstOrDefault(x => x.Email == User.Identity.Name);
                if(user == null)
                {
                    return Unauthorized();
                }
                users = users.Where(x => x.DepartamentId == user.DepartamentId);
            }
            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(x => x.Departament)
                .Include(x => x.Status)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (role == "client" || role == "superclient")
            {
                return Ok("{\"fullName\":\""+user.FullName+"\"}");
            }
            

            return Ok(user);
        }

        // PUT: api/User/5
        [Authorize(Roles = "admin,superuser,user")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] int id, PutUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = StaticHelper.GetCurrentRole(User);

            var oldUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            var curUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if(curUser == null || oldUser == null)
            {
                return BadRequest();
            }
            
            if(role == "user" && oldUser.Id != curUser.Id)
            {
                return BadRequest();
            }
            if(role == "superuser" && oldUser.DepartamentId != curUser.DepartamentId)
            {
                return BadRequest();
            }
            if(user.Email != null)
            {
                var cl = _context.Clients.FirstOrDefault(x => x.Email == user.Email);
                var us = _context.Users.FirstOrDefault(x => x.Email == user.Email);
                if (cl == null && us == null)
                {
                    oldUser.Email = user.Email;
                }
                else
                {
                    return BadRequest("Provided email is occupied by someone.");
                }
            }
            var hasher = new PasswordHasher<AppUser>();
            if (user.Password != null)
            {
                if (hasher.VerifyHashedPassword(new AppUser(), oldUser.Password, user.Password)
                    == PasswordVerificationResult.Success)
                {
                    oldUser.Password = hasher.HashPassword(new AppUser(), user.Password);
                }
            }
            if (role == "admin")
            {
                oldUser.DepartamentId = user.DepartamentId ?? oldUser.DepartamentId;
            }
            oldUser.StatusId = user.StatusId ?? oldUser.StatusId;
            oldUser.FullName = user.FullName ?? oldUser.FullName;
            if(role == "admin" && user.RoleId != null)
            {
                if(user.RoleId != 2)
                {
                    user.RoleId = 1;
                }
                oldUser.RoleId = (int)user.RoleId;
            }
            _context.Entry(oldUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/User
        [Authorize(Roles = "admin,superuser")]
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = StaticHelper.GetCurrentRole(User);
            
            if(role == "superuser")
            {
                var cUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
                if(cUser.DepartamentId != user.DepartamentId)
                {
                    return BadRequest("Can't create user for other departament");
                }
            }
            if(user.RoleId != 2 && user.RoleId != 3)
            {
                user.RoleId = 1;
            }
            user.StatusId = 1;

            var cl = _context.Clients.FirstOrDefault(x => x.Email == user.Email);
            var us = _context.Users.FirstOrDefault(x => x.Email == user.Email);
            if (cl != null || us != null)
            {
                return BadRequest("Provided email is occupied by someone.");
            }

            var oldPass = user.Password;
            user.Password = new PasswordHasher<AppUser>().HashPassword(new AppUser(), user.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Invalid data. Can't register user.");
            }

            await StaticHelper.SendEmailAsync(user.Email, "Регистрация", 
                "Спасибо за регистрацию на сервисе HelpDesk, вам был присвоен пароль : " + oldPass);

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Include(x => x.Departament)
                .Include(x => x.Status)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete user because other entities have reference to it");
            }

            return Ok(user);
        }

 
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}