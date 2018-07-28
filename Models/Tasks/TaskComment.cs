using HelpDesk.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Task
{
    public class TaskComment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        [Required]
        [ForeignKey("TicketTask")]
        public int TaskId { get; set; }
        [JsonIgnore]
        public TicketTask TicketTask { get; set; }
    }
}
