using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Additional
{
    public class RegistrationToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime Opened { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public Role Role { get; set; }

    }
}
