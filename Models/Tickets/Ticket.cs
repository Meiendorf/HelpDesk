using HelpDesk.Models.Common;
using HelpDesk.Models.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using HelpDesk.Models.Users;
using Newtonsoft.Json;

namespace HelpDesk.Models.Tickets
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Content { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public DateTime DateModified { get;set; }

        [Required]
        public int TypeId { get; set; }
        public TicketType Type { get; set; }

        public int StatusId { get; set; }
        public TicketStatus Status { get; set; }

        [Required]
        public int PriorityId { get; set; }
        public Priority Priority { get; set; }

        public int? ObjectiveId { get; set; }
        public Objective Objective { get; set; }

        [Required]
        public int ClientId { get; set; }
        [JsonIgnore]
        public Client Client { get; set; }

        public int? UserInitId { get; set; }
        [JsonIgnore]
        public User UserInit { get; set; }

        [Required]
        public int? DepartamentId { get; set; }
        public Departament Departament { get; set; }

        public int? UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        
    }
}
