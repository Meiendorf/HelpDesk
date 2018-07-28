using HelpDesk.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Users
{
    public class User : AppUser
    {
        [Required]
        [ForeignKey("Departament")]
        public int DepartamentId { get; set; }
        public Departament Departament { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }
        public UserStatus Status { get; set; }
    }
}
