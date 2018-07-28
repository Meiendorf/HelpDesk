using HelpDesk.Models.Clients;
using HelpDesk.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models
{
    public class UserAndClient
    {
        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User User { get; set; }

        public bool ShouldSerializeClient()
        {
            return false;
        }

        public bool ShouldSerializeUser()
        {
            return false;
        }
    }
}
