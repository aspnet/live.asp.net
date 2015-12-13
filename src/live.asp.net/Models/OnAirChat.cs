// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace live.asp.net.Models
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
