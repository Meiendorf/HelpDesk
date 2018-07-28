using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Tickets
{
    public class PutTicket
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int? TypeId { get; set; }
        public int? StatusId { get; set; }
        public int? PriorityId { get; set; }
        public int? ObjectiveId { get; set; }
        public int? DepartamentId { get; set; }
        public int? UserId { get; set; }
    }
}
