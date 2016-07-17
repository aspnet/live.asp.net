// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using live.asp.net.Models;
using System.Collections.Generic;
using System.Text;

namespace live.asp.net.ViewModels
{
    public class ShowViewModel
    {
        public Show Show { get; set; }
        public ShowDetails ShowDetails { get; set; }

        public bool HasShowDetailDescription
        {
            get { return ShowDetails != null && !string.IsNullOrWhiteSpace(ShowDetails.Description); }
        }

        public bool HasTableOfContents
        {
            get { return ShowDetails != null && ShowDetails.TableOfContents != null && ShowDetails.TableOfContents.Groups.Count() > 0; }
        }

        public IEnumerable<TableOfContentsGroupViewModel> TableOfContentsGroups
        {
            get
            {
                return ShowDetails.TableOfContents.Groups.Select(g => new TableOfContentsGroupViewModel(g));
            }
        }
    }

    public class TableOfContentsGroupViewModel
    {
        private TableOfContentsGroup _group;

        public string Header
        {
            get { return _group.GroupHeader; }
        }

        public IEnumerable<TableOfContentsItemViewModel> Items
        {
            get
            {
                return _group.Items.Select(i => new TableOfContentsItemViewModel(i));
            }
        }
        public TableOfContentsGroupViewModel(TableOfContentsGroup group)
        {
            _group = group;
        }
    }

    public class TableOfContentsItemViewModel
    {
        private TableOfContentsItem _item;

        public string Description
        {
            get { return _item.Description; }
        }
        public string PositionAsUrlParameter
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (_item.Position.Hours > 0)
                    sb.Append(_item.Position.Hours + "h");

                if (_item.Position.Minutes > 0)
                    sb.Append(_item.Position.Minutes + "m");
                else if (sb.Length > 0)
                    sb.Append("0m");

                if (_item.Position.Seconds > 0)
                    sb.Append(_item.Position.Seconds + "s");

                return sb.ToString();
            }
        }

        public string PositionAsText
        {
            get
            {
                if (_item.Position.Hours > 0)
                    return _item.Position.ToString(@"hh\:mm\:ss");

                return _item.Position.ToString(@"mm\:ss");
            }
        }

        public TableOfContentsItemViewModel(TableOfContentsItem item)
        {
            _item = item;
        }
    }
}