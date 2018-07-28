using HelpDesk.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Tickets
{
    public class TicketComment : UserAndClient
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        [JsonIgnore]
        public Ticket Ticket { get; set; }
    }
}
