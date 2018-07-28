using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Articles
{
    public class ArticleAttachment
    {
        public int Id { get; set; }

        [Required]
        public string Path { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        [Required]
        [ForeignKey("Article")]
        public int ArticleId { get; set; }
        [JsonIgnore]
        public Article Article { get; set; }
    }
}
