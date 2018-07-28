using HelpDesk.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Articles
{
    public class Article
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime Date { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }

        [Required]
        [ForeignKey("Type")]
        public int TypeId { get; set; }
        public ArticleType Type { get; set; }

        [Required]
        [ForeignKey("Section")]
        public int SectionId { get; set; }
        public ArticleSection Section { get; set; }
    }
}
