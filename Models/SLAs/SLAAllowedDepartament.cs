using HelpDesk.Models.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.SLAs
{
    public class SLAAllowedDepartament : UniqueField
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("SLA")]
        public int SLAId { get; set; }
        public SLA Sla { get; set; }

        [Required]
        [ForeignKey("Departament")]
        public int DepartamentId { get; set; }
        public Departament Departament { get; set; }
    }
}
