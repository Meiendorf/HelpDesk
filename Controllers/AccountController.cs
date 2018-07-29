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
        [ActionName("changeRole")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeRole([FromForm] string username, [FromForm] int roleId)
        {
            var client = await db.Clients.FirstOrDefaultAsync(x => x.Email == username);
            if(client != null)
            {
                if (roleId > 3)
                {
                    client.RoleId = roleId;
                }
                else
                {
                    return BadRequest("Invalid client role!");
                }
                db.Clients.Update(client);
            }
            else
            {
                var user = await db.Users.FirstOrDefaultAsync(x => x.Email == username);
                if(user == null)
                {
                    return BadRequest("No user was found by provided username!");
                }
                if (roleId < 4 && roleId > 0)
                {
                    user.RoleId = roleId;
                }
                else
                {
                    return BadRequest("Invalid user role!");
                }
                db.Users.Update(user);
            }
            await db.SaveChangesAsync();
            await CloseAllSessionsAsync(username);
            return Ok();
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
            var response = await GenerateTokenResponse(identity);
  
            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }
       
        [ActionName("refresh")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken)
        {
            if (refreshToken == null)
            {
                return BadRequest("Refresh token is required!");
            }
            var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (token == null || !token.IsActive)
            {
                return BadRequest("Token is invalid!");
            }
            AppUser user;
            if(token.UserId != null)
            {
                user = await db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == token.UserId);
            }
            else
            {
                user = await db.Clients.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == token.ClientId);
            }
            if(user == null)
            {
                return BadRequest("Token is invalid!");
            }
            token.IsActive = false;
            db.RefreshTokens.Update(token);
            await db.SaveChangesAsync();
            var time = DateTime.Now - token.Created;
            if(time.TotalMinutes > token.Expire)
            {
                return BadRequest("Token expired!");
            }
            return new JsonResult(await GenerateTokenResponse(GetIdentityObject(user)));
        }
        
        [ActionName("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout([FromForm] string refreshToken)
        {
            if (refreshToken == null)
            {
                return BadRequest("Refresh token is required!");
            }
            var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (token == null || !token.IsActive)
            {
                return BadRequest("Token is invalid!");
            }
            token.IsActive = false;
            db.RefreshTokens.Update(token);
            await db.SaveChangesAsync();
            return Ok();
        }

        [ActionName("closeall")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CloseAllSessions()
        {
            if(!await CloseAllSessionsAsync(User.Identity.Name))
            {
                return BadRequest();
            }
            return Ok();
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

        private IdentityObject GetIdentity(string email, string password)
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
                return GetIdentityObject(user);
            }
            return null;
        }
        private async Task<bool> CloseAllSessionsAsync(string username)
        {
            AppUser user = await db.Users.FirstOrDefaultAsync(x => x.Email == username);
            var isClient = false;
            if (user == null)
            {
                isClient = true;
                user = await db.Clients.FirstOrDefaultAsync(x => x.Email == username);
            }
            if (user == null)
            {
                return false;
            }
            if (isClient)
            {
                db.Database.ExecuteSqlCommand("UPDATE RefreshTokens SET IsActive ='False' WHERE ClientId=" + user.Id);

            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE RefreshTokens SET IsActive='False' WHERE UserId=" + user.Id);
            }
            return true;
        }
        private async Task<TokenResponse> GenerateTokenResponse(IdentityObject identity)
        {
            var jwtSettings = config.GetSection("JWTAuthentication");
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    notBefore: now,
                    claims: identity.Claims.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(Convert.ToDouble(jwtSettings["Lifetime"]))),
                    signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                                Encoding.ASCII.GetBytes(jwtSettings["Key"])), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var refreshToken = new RefreshToken()
            {
                IpAdress = HttpContext.Connection.RemoteIpAddress.ToString(),
                Expire = Convert.ToInt32(jwtSettings["RefreshLifetime"]),
                IsActive = true,
                Token = Guid.NewGuid().ToString("N"),
                Created = DateTime.Now
            };
            if (identity.User.RoleId > 3)
            {
                refreshToken.ClientId = identity.User.Id;
            }
            else
            {
                refreshToken.UserId = identity.User.Id;
            }
            db.RefreshTokens.Add(refreshToken);
            await db.SaveChangesAsync();
            var response = new TokenResponse
            {
                access_token = encodedJwt,
                role = identity.User.Role.Name,
                username = identity.User.Email,
                refresh_token = refreshToken.Token
            };
            return response;
        }
        private IdentityObject GetIdentityObject (AppUser user)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role.Name)
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            return new IdentityObject()
            {
                Claims = claimsIdentity,
                User = user
            };
        }
       
    }
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string role { get; set; }
        public string username { get; set; }
    }
    public class IdentityObject
    {
        public ClaimsIdentity Claims { get; set; }
        public AppUser User { get; set; }
    }
}