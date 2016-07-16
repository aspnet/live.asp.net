using System;
using System.Collections.Generic;

namespace live.asp.net.Models
{
    public class TableOfContents
    {
        public IEnumerable<TableOfContentsGroup> Groups { get; set; }
    }

    public class TableOfContentsGroup
    {
        public string GroupHeader { get; set; }
        public IEnumerable<TableOfContentsItem> Items { get; set; }
    }

    public class TableOfContentsItem
    {
        public TimeSpan Position { get; set; }
        public string Description { get; set; }
    }
}