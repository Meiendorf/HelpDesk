using HelpDesk.Models.Clients;
using HelpDesk.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Notifications
{
    public class Notification : UserAndClient
    {
        public int Id { get; set; }

        public bool UseEmail { get; set; }
        public bool UseSms { get; set; }

        [Required]
        [ForeignKey("EventType")]
        public int EventTypeId { get; set; }
        public EventType EventType { get; set; }
    }
}
