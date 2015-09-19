// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class AdminViewModel
    {
        [Display(Name = "Streaming Embed URL", Description = "URL for embedding the live show")]
        [DataType(DataType.Url)]
        public string LiveShowEmbedUrl { get; set; }

        [Display(Name = "Next Show Date/time", Description = "Exact date and time of the next live show")]
        [DateAfterNow(TimeZoneId = "Pacific Standard Time")]
        public DateTime? NextShowDatePst { get; set; }

        [Display(Name = "Standby Message", Description = "Message to show on home page during show standby")]
        public string AdminMessage { get; set; }

        public string SuccessMessage { get; set; }

        public bool ShowSucessMessage => !string.IsNullOrEmpty(SuccessMessage);

        public AppSettings AppSettings { get; set; }

        public string EnvironmentName { get; set; }
    }
}
