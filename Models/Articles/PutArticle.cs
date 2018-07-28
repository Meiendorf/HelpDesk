using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Articles
{
    public class PutArticle
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public int? TypeId { get; set; }
        public int? SectionId { get; set; }
    }
}
