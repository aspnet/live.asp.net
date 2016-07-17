// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace live.asp.net.Models
{
    public class ShowDetails
    {
        public string ShowId { get; set; }
        public string Description { get; set; }
        public TableOfContents TableOfContents { get; set; }
    }
}