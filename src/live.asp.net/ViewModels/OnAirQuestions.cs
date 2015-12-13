using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net.ViewModels
{
    public class OnAirQuestions
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Question { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool Answering { get; set; }
        public bool Answered { get; set; }
        public int? Vote { get; set; }
        public string AdminMenu { get; set; }
    }
}
