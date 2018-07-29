using HelpDesk.Models.Clients;
using HelpDesk.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Additional
{
    public class RefreshToken
    {
        public int Id { get; set; }
        [Required]
        public string Token { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Client")]
        public int? ClientId { get; set; }
        public Client Client { get; set; }

        public string IpAdress { get; set; }
        public int Expire { get; set; }
        public DateTime Created { get; set; }
    }
}
