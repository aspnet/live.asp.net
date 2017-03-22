// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace live.asp.net.Models
{
    public interface ILiveShowDetails
    {
        string LiveShowEmbedUrl { get; set; }

        string LiveShowHtml { get; set; }

        DateTime? NextShowDateUtc { get; set; }

        string AdminMessage { get; set; }
    }
}
