using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net.ViewModels
{
    public class OnAirChat
    {
        public string UserName { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }

        public string TimeStampHuman
        {
            get { return TimeStamp.ToString("MM-dd-yyyy h:mm:ss tt "); }
        }
    }
}
