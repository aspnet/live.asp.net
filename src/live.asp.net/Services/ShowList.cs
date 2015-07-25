using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using live.asp.net.Models;

namespace live.asp.net.Services
{
    public class ShowList
    {
        public IList<Show> Shows { get; set; }

        public string MoreShowsUrl { get; set; }
    }
}
