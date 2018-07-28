using HelpDesk.Models;
using HelpDesk.Models.Additional;
using HelpDesk.Models.Notifications;
using HelpDesk.Models.Task;
using HelpDesk.Models.Tickets;
using HelpDesk.Models.Users;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HelpDesk.Services
{
    public static class StaticHelper
    {
        public static string GetCurrentRole(ClaimsPrincipal User)
        {
            var roleCont = User.Claims.FirstOrDefault(x => x.Type == ClaimsIdentity.DefaultRoleClaimType);
            if (roleCont == null)
            {
                return null;
            }
            else
            {
                return roleCont.Value;
            }
        }
        public static bool IsEmailInBase(string email, AppDbContext db)
        {
            var us = db.Users.FirstOrDefault(x => x.Email == email);
            var cl = db.Clients.FirstOrDefault(x => x.Email == email);
            var tk = db.RegistrationTokens.FirstOrDefault(x => x.Email == email);
            if(us == null && cl == null && tk == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация HelpDesk", "natsuki-bot@rambler.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.rambler.ru", 587, false);
                await client.AuthenticateAsync("natsuki-bot@rambler.ru", "kiritorito1110111");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
        public static async Task<bool> RaiseEvent(EventTypes type, object data, AppDbContext db)
        {
            var _event = new Event()
            {
                Date = DateTime.Now,
                TypeId = (int)type
            };
            var subscribers = db.Notifications
                .Include(x => x.User).ThenInclude(x => x.Role)
                .Include(x => x.Client).ThenInclude(x => x.Role)
                .Where(x => x.EventTypeId == (int)type);

            string subject;
            switch (type)
            {
                case EventTypes.TicketAdded:
                    subject = "Новый тикет!";
                    break;
                case EventTypes.TicketChanged:
                    subject = "Обновления тикета!";
                    break;
                case EventTypes.TicketComment:
                    subject = "Новый комментарий к тикету!";
                    break;
                case EventTypes.TaskAdded:
                    subject = "Новое задание!";
                    break;
                case EventTypes.TaskChanged:
                    subject = "Обновление задания!";
                    break;
                case EventTypes.TaskCommented:
                    subject = "Новый комментарий к заданию!";
                    break;
                default:
                    return false;
            }
            if ((int)type < 3)
            {
                var ticket = data as Ticket;
                if (ticket == null)
                {
                    return false;
                }
                switch (type)
                {
                    case EventTypes.TicketAdded:
                        _event.Description = $"Тикет с номером {ticket.Id} был добавлен в систему.";
                        break;
                    case EventTypes.TicketChanged:
                        _event.Description = $"Тикет с номером {ticket.Id} был обновлен.";
                        break;
                    case EventTypes.TicketComment:
                        _event.Description = $"Тикет с номером {ticket.Id} был прокомментирован.";
                        break;
                    default:
                        return false;
                }
                foreach (var sub in subscribers)
                {
                    var isOk = 0;
                    AppUser user = sub.Client as AppUser ?? sub.User as AppUser;
                    if (user.Role.Name == "client" || user.Role.Name == "superclient")
                    {
                        if (ticket.ClientId == user.Id)
                        {
                            isOk++;
                        }
                    }
                    if (user.Role.Name == "user" || user.Role.Name == "admin")
                    {
                        if (ticket.UserId == user.Id)
                        {
                            isOk++;
                        }
                    }
                    if (user.Role.Name == "superuser")
                    {
                        if (ticket.ClientId == user.Id || ((ticket.DepartamentId ==
                            (user as User).DepartamentId) && (ticket.UserId == null)))
                        {
                            isOk++;
                        }
                    }
                    if(isOk == 1)
                    {
                        await SendEmailAsync(user.Email, subject, _event.Description);
                    }
                }
            }
            else
            {
                var task = data as TicketTask;
                if (task == null)
                {
                    return false;
                }
                foreach (var sub in subscribers)
                {
                    var isOk = 0;
                    User user = sub.User;
                    if (user == null)
                    {
                        return false;
                    }
                    if (user.Id == task.UserId)
                    {
                        isOk++;
                    }
                    if (isOk == 1)
                    {
                        await SendEmailAsync(user.Email, subject, _event.Description);
                    }
                }
            }
            try
            {
                db.Events.Add(_event);
                await db.SaveChangesAsync();
            }
            catch
            {
                Console.WriteLine("Error while adding event to database");
            }
            return true;
        }
        public static bool CheckTicketByRole(string role, Ticket ticket, string email, AppDbContext db)
        {

            var user = db.Users.FirstOrDefault(x => x.Email == email);
            if (((role == "client") && (ticket.Client.Email != email)) ||
               ((role == "superclient") && (ticket.Client.Company.Name !=
                 db.Clients.Include(x => x.Company).First(x => x.Email == email).Company.Name)) ||
               ((role == "user") && ((ticket.DepartamentId != user.DepartamentId) &&
               ((ticket.UserId != null) || (ticket.UserId != user.Id)))) ||
               ((role == "superuser") && (ticket.DepartamentId != user.DepartamentId)))
            {
                return false;
            }

            return true;
        }
        public static bool CheckTaskByRole(string role, TicketTask task, string email, AppDbContext db)
        {
            var user = db.Users.FirstOrDefault(x => x.Email == email);
            if(role == "admin")
            {
                return true;
            }
            if(role == "user")
            {
                if(task.UserId == user.Id)
                {
                    return true;
                }
            }
            if(role == "superuser")
            {
                if(task.User.DepartamentId == user.DepartamentId)
                {
                    return true;
                }
            }
            return false;
        }

    }

    public class PropertyRenameAndIgnoreSerializerContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, HashSet<string>> _ignores;
        private readonly Dictionary<Type, Dictionary<string, string>> _renames;

        public PropertyRenameAndIgnoreSerializerContractResolver()
        {
            _ignores = new Dictionary<Type, HashSet<string>>();
            _renames = new Dictionary<Type, Dictionary<string, string>>();
        }

        public void IgnoreProperty(Type type, params string[] jsonPropertyNames)
        {
            if (!_ignores.ContainsKey(type))
                _ignores[type] = new HashSet<string>();

            foreach (var prop in jsonPropertyNames)
                _ignores[type].Add(prop);
        }

        public void RenameProperty(Type type, string propertyName, string newJsonPropertyName)
        {
            if (!_renames.ContainsKey(type))
                _renames[type] = new Dictionary<string, string>();

            _renames[type][propertyName] = newJsonPropertyName;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName))
                property.ShouldSerialize = i => false;

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        private bool IsIgnored(Type type, string jsonPropertyName)
        {
            if (!_ignores.ContainsKey(type))
                return false;

            return _ignores[type].Contains(jsonPropertyName);
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }
}
