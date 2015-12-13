using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net.ViewModels
{
    public class ChatAndQuestions
    {
        public ICollection<OnAirChat> chats { get; set; }
        public ICollection<OnAirQuestions> questions { get; set; }
        public string LastTime { get; set; }
        public int Delete { get; set; }
    }
}
