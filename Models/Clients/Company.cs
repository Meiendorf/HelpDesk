using HelpDesk.Models.SLAs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Clients
{
    public class Company : TitleClass
    {
        [Required]
        [ForeignKey("SLA")]
        public int SLAId { get; set; }
        public SLA Sla { get; set; }
    }
}
