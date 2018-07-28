using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Clients;
using Microsoft.AspNetCore.Authorization;
using HelpDesk.Services;
using HelpDesk.Models;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Controllers.Clients
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Client
        [HttpGet]
        public IActionResult GetClients([FromQuery] int? companyid)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var clients = _context.Clients.Include(x => x.Company).Include(x => x.Role) as IQueryable<Client>;
            
            if(role == "superclient")
            {
                var logClient = _context.Clients.FirstOrDefault(x => x.FullName == User.Identity.Name);
                if(logClient == null)
                {
                    return Unauthorized();
                }
                if(logClient.CompanyId != companyid)
                {
                    return Unauthorized();
                }
            }
            if(companyid != null)
            {
                clients = clients.Where(x => x.CompanyId == companyid);
            }
            return Ok(_context.Clients.Include(x => x.Company).Include(x => x.Role));
        }

        // GET: api/Client/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient([FromRoute] int id)
        {
            var role = StaticHelper.GetCurrentRole(User);

            var client = await _context.Clients
                .Include(x => x.Company)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (client == null)
            {
                return NotFound();
            }

            if (role == "client" || role =="superclient")
            {
                var logClient = _context.Clients.FirstOrDefault(x => x.Email == User.Identity.Name);
                if(logClient == null)
                {
                    return Unauthorized();
                }
                if(role == "client" && logClient.Id != client.Id)
                {
                    return Forbid();
                }
                if(role == "superclient" && logClient.CompanyId != client.CompanyId)
                {
                    return Forbid();
                }
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(client);
        }

        // PUT: api/Client/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClient([FromRoute] int id, [FromBody] PutClient client)
        {
            var role = StaticHelper.GetCurrentRole(User);
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var oldClient = _context.Clients.FirstOrDefault(x => x.Id == id);
            if(oldClient == null)
            {
                return NotFound();
            }

            if (role == "client" || role == "superclient")
            {
                var user = _context.Clients.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);
                if(user == null)
                {
                    return Unauthorized();
                }
                if(user.Id != oldClient.Id)
                {
                    return Forbid();
                }
            }
            if (client.Email != null)
            {
                var cl = _context.Clients.FirstOrDefault(x => x.Email == client.Email);
                var us = _context.Users.FirstOrDefault(x => x.Email == client.Email);
                if (cl == null && us == null)
                {
                    oldClient.Email = client.Email;
                }
                else
                {
                    return BadRequest("Provided email is occupied by someone.");
                }
            }
            var hasher = new PasswordHasher<AppUser>();
            if (client.Password != null)
            {
                if (hasher.VerifyHashedPassword(new AppUser(), oldClient.Password, client.Password)
                    == PasswordVerificationResult.Success)
                {
                    oldClient.Password = hasher.HashPassword(new AppUser(), client.Password);
                }
            }
            oldClient.FullName = client.FullName ?? oldClient.FullName;
            oldClient.CompanyId = client.CompanyId ?? oldClient.CompanyId;

            _context.Entry(oldClient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest("Provided data was invalid!");
                }
            }

            return NoContent();
        }

        [Authorize(Roles = "admin,user,superuser")]
        [HttpPost]
        public async Task<IActionResult> PostClient([FromBody] Client client)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var us = _context.Users.FirstOrDefault(x => x.Email == client.Email);
            var cl = _context.Clients.FirstOrDefault(x => x.Email == client.Email);
            if(us != null || cl != null)
            {
                return BadRequest("Provided email is occupied by someone.");
            }
            var oldPass = client.Password;
            client.Password = new PasswordHasher<AppUser>().HashPassword(new AppUser(), client.Password);
            if(client.RoleId != 5)
            {
                client.RoleId = 4;
            }
            try
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Some provided data was invalid!");
            }

            await StaticHelper.SendEmailAsync(client.Email, "Регистрация",
                "Спасибо за регистрацию на платформе HelpDesk. Вам присовоен пароль : " + oldPass);

            return CreatedAtAction("GetClient", new { id = client.Id }, client);
        }

        // DELETE: api/Client/5
        [Authorize(Roles = "admin,user,superuser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = await _context.Clients
                .Include(x => x.Company)
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (client == null)
            {
                return NotFound();
            }
            try
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Can't delete client because some other entities have reference to it");
            }

            return Ok(client);
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}