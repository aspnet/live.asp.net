// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace live.asp.net.Models
{
    public class OnAirChatAndQuestions
    {
        public ICollection<OnAirChat> chats { get; set; }

        public ICollection<OnAirQuestions> questions { get; set; }

        public string LastTime { get; set; }

        public int Delete { get; set; }
    }
}
