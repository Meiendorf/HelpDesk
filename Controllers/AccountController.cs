using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HelpDesk.Models;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Clients;
using HelpDesk.Models.Users;
using HelpDesk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace HelpDesk.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private AppDbContext db { get; set; }
        private IConfiguration config { get; set; }

        public AccountController(AppDbContext context, IConfiguration configuration)
        {
            db = context;
            config = configuration;
        }
        [ActionName("token")]
        [HttpPost]
        public async Task Token([FromForm] string username, [FromForm] string password)
        {
            if(username == null || password == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("No data was provided.");
                return;
            }
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid username or password.");
                return;
            }
            var jwtSettings = config.GetSection("JWTAuthentication");
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings["Lifetime"]))),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes(jwtSettings["Key"])), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                role = identity.Claims.First(x => x.Type == ClaimsIdentity.DefaultRoleClaimType).Value,
                username = identity.Name
            };

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        [ActionName("genuserlink")]
        [Authorize(Roles = "admin,superuser")]
        [HttpPost]
        public async Task<IActionResult> GenerateUserLink([FromQuery] int? roleId, [FromQuery] string email)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if(roleId != 2 || role == "superuser")
            {
                roleId = 1;
            }
            if(email == null)
            {
                return BadRequest("Email is required!");
            }
            if(StaticHelper.IsEmailInBase(email, db))
            {
                return BadRequest("Provided email is already registred!");
            }
            var regLink = new RegistrationToken()
            {
                RoleId = (int)roleId,
                Type = "User",
                Email = email,
                Token = Guid.NewGuid().ToString("N"),
                Opened = DateTime.Now
            };

            db.RegistrationTokens.Add(regLink);
            await db.SaveChangesAsync();

            return Ok(regLink);
        }

        [ActionName("reguser")]
        [HttpPost]
        public async Task<IActionResult> RegUserByToken([FromBody] User user, [FromQuery] string token)
        {
            if(token == null)
            {
                return BadRequest("Registration token required!");
            }
            var regToken = db.RegistrationTokens.FirstOrDefault(x => x.Token == token);
            if(regToken == null)
            {
                return BadRequest("Invalid registration token!");
            }
            if(regToken.Type != "User")
            {
                return BadRequest("Invalid token type!");
            }
            var span = DateTime.Now - regToken.Opened;
            if(span.TotalHours > 24)
            {
                db.RegistrationTokens.Remove(regToken);
                return BadRequest("Token has expired");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var role = StaticHelper.GetCurrentRole(User);

            user.StatusId = 1;
            user.Email = regToken.Email;
            user.RoleId = regToken.RoleId;

            var oldPass = user.Password;
            user.Password = new PasswordHasher<AppUser>().HashPassword(new AppUser(), user.Password);
            try
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Invalid data. Can't register user.");
            }
           
            await StaticHelper.SendEmailAsync(user.Email, "Регистрация",
                "Спасибо за регистрацию на сервисе HelpDesk "+regToken.Email+"! Вам присвоен пароль : " + oldPass);

            db.RegistrationTokens.Remove(regToken);
            await db.SaveChangesAsync();

            return CreatedAtAction("GetUser", "User", new { id = user.Id }, user);
        }

        [ActionName("genclientlink")]
        [Authorize(Roles = "admin,superuser,user")]
        [HttpPost]
        public async Task<IActionResult> GenerateClientLink([FromQuery] int? roleId, [FromQuery] string email)
        {
            var role = StaticHelper.GetCurrentRole(User);

            if (roleId != 5)
            {
                roleId = 4;
            }
            if (email == null)
            {
                return BadRequest("Email is required!");
            }
            if (StaticHelper.IsEmailInBase(email, db))
            {
                return BadRequest("Provided email is already registred or occupied!");
            }
            var regLink = new RegistrationToken()
            {
                RoleId = (int)roleId,
                Type = "Client",
                Email = email,
                Token = Guid.NewGuid().ToString("N"),
                Opened = DateTime.Now
            };

            db.RegistrationTokens.Add(regLink);
            await db.SaveChangesAsync();

            return Ok(regLink);
        }

        [ActionName("regclient")]
        [HttpPost]
        public async Task<IActionResult> RegClientByToken([FromBody] Client client, [FromQuery] string token)
        {
            if (token == null)
            {
                return BadRequest("Registration token required!");
            }
            var regToken = db.RegistrationTokens.FirstOrDefault(x => x.Token == token);
            if (regToken == null)
            {
                return BadRequest("Invalid registration token!");
            }
            if (regToken.Type != "Client")
            {
                return BadRequest("Invalid token type!");
            }
            var span = DateTime.Now - regToken.Opened;
            if (span.TotalHours > 24)
            {
                db.RegistrationTokens.Remove(regToken);
                return BadRequest("Token has expired");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = StaticHelper.GetCurrentRole(User);
       
            var oldPass = client.Password;
            client.Password = new PasswordHasher<AppUser>().HashPassword(new AppUser(), client.Password);
            client.Email = regToken.Email;
            client.RoleId = regToken.RoleId;

            try
            {
                db.Clients.Add(client);
                await db.SaveChangesAsync();
            }
            catch
            {
                return BadRequest("Invalid data. Can't register client.");
            }

            await StaticHelper.SendEmailAsync(client.Email, "Регистрация",
                "Спасибо за регистрацию на сервисе HelpDesk " + regToken.Email + "! Вам присвоен пароль : " + oldPass);

            db.RegistrationTokens.Remove(regToken);
            await db.SaveChangesAsync();

            return CreatedAtAction("GetClient", "Client", new { id = client.Id }, client);
        }

        private ClaimsIdentity GetIdentity(string email, string password)
        {
            AppUser user = db.Clients.Include(x => x.Role).FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                user = db.Users.Include(x => x.Role).FirstOrDefault(x => x.Email == email);
            }
            var hasher = new PasswordHasher<AppUser>();
            if (user != null)
            {
                if(hasher.VerifyHashedPassword(new AppUser(),user.Password, password) == PasswordVerificationResult.Failed)
                {
                    return null;
                }
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);
                return claimsIdentity;
            }
            return null;
        }
       
    }
}