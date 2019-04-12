// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using live.asp.net.Models;

namespace live.asp.net.ViewModels
{
    public class AdminViewModel
    {
        [Display(Name = "Live Show URL", Description = "URL for the live show")]
        [DataType(DataType.Url)]
        public string? LiveShowEmbedUrl { get; set; }
        
        [Display(Name = "Live Show HTML", Description = "HTML content for the live show")]
        [DataType(DataType.MultilineText)]
        public string? LiveShowHtml { get; set; }

        [Display(Name = "Next Show Date/time", Description = "Exact date and time of the next live show in Pacific Time")]
        [DateAfterNow(TimeZoneId = "Pacific Standard Time")]
        public DateTime? NextShowDatePst { get; set; }

        [Display(Name = "Standby Message", Description = "Message to show on home page during show standby")]
        public string? AdminMessage { get; set; }

        public string? NextShowDateSuggestionPstAM { get; set; }

        public string? NextShowDateSuggestionPstPM { get; set; }

        public string? SuccessMessage { get; set; }

        public bool ShowSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);

        public AppSettings? AppSettings { get; set; }

        public string? EnvironmentName { get; set; }
    }
}
