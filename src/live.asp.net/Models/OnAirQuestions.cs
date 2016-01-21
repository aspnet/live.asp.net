// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace live.asp.net.Models
{
    public class OnAirQuestions
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Question { get; set; }

        public DateTime TimeStamp { get; set; }

        public bool Answering { get; set; }

        public bool Answered { get; set; }

        public int Vote { get; set; }
    }
}
