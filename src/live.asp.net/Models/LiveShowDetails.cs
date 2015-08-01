// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace live.asp.net.Models
{
    public class LiveShowDetails
    {
        public string LiveShowEmbedUrl { get; set; }

        public DateTime? NextShowDateUtc { get; set; }

        public string AdminMessage { get; set; }
    }
}
