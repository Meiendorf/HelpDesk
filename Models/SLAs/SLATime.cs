using HelpDesk.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.SLAs
{
    public class SLATime : UniqueField
    {
        public int Id { get; set; }

        [Required]
        public double ResponseTime { get; set; }

        [Required]
        public double WorkTime { get; set; }

        [Required]
        [ForeignKey("SLA")]
        public int SLAId { get; set; }
        public SLA Sla { get; set; }

        [Required]
        [ForeignKey("Priority")]
        public int PriorityId { get; set; }
        public Priority Priority { get; set; }
    }
}
