using HelpDesk.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.SLAs
{
    public class SLAAllowedObjective : UniqueField
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("SLA")]
        public int SLAId { get; set; }
        public SLA Sla { get; set; }

        [Required]
        [ForeignKey("Objective")]
        public int ObjectiveId { get; set; }
        public Objective Objective { get; set; }
    }
}
