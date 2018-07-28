using HelpDesk.Models.Tickets;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.SLAs
{
    public class SLAAllowedType : UniqueField
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("SLA")]
        public int SLAId { get; set; }
        public SLA Sla { get; set; }

        [Required]
        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public TicketType Type { get; set; }
    }
}
