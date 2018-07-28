using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models
{
    public class UniqueField
    {
        [JsonIgnore]
        public string Unique { get; set; }
    }
}
