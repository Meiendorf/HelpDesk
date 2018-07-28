using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Users
{
    public class PutUser
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int? RoleId { get; set; }
        public int? DepartamentId { get; set; }
        public int? StatusId { get; set; }
    }
}
