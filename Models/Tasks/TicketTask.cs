using HelpDesk.Models.Tickets;
using HelpDesk.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Task
{
    public class TicketTask : TitleClass
    {
        [Required]
        public string Content { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }

        [Required]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        [Required]
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        [JsonIgnore]
        public Ticket Ticket { get; set; }

        [ForeignKey("Status")]
        public int StatusId { get; set; }
        public TicketStatus Status { get; set; }
    }
}
