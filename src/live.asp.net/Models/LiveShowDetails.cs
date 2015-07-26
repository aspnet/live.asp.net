using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net.Models
{
    public class LiveShowDetails
    {
        public int Id { get; set; }

        public string LiveShowEmbedUrl { get; set; }

        public DateTime? NextShowDate { get; set; }
    }
}
