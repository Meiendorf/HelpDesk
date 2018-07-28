using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Task
{
    public class PutTicketTask
    {
        public string Name { get; set; }

        public string Content { get; set; }

        public int? UserId { get; set; }

        public int? StatusId { get; set; }
    }
}
