using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Tickets
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Type { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        [JsonIgnore]
        public Ticket Ticket { get; set; }

    }
}
