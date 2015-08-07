// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdminViewModel.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using live.asp.net.Models;

    /// <summary>
    ///     The administration view model class.
    /// </summary>
    public class AdminViewModel
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the message to show on home page in case of delay in
        ///     starting live show, etc.
        /// </summary>
        /// <value>
        ///     The message to show on home page in case of delay in starting live
        ///     show, etc.
        /// </value>
        [Display(Name = "Admin Message", Description = "Message to show on home page in case of delay in starting live show, etc.")]
        public string AdminMessage { get; set; }

        /// <summary>
        ///     Gets or sets the application settings.
        /// </summary>
        /// <value>
        ///     The application settings.
        /// </value>
        public AppSettings AppSettings { get; set; }

        /// <summary>
        ///     Gets or sets the name of the environment.
        /// </summary>
        /// <value>
        ///     The name of the environment.
        /// </value>
        public string EnvironmentName { get; set; }

        /// <summary>
        ///     Gets or sets the URL for embedding the live show.
        /// </summary>
        /// <value>
        ///     The URL for embedding the live show.
        /// </value>
        [Display(Name = "Streaming Embed URL", Description = "URL for embedding the live show")]
        [DataType(DataType.Url)]
        public string LiveShowEmbedUrl { get; set; }

        /// <summary>
        ///     Gets or sets the exact date and time of the next live show.
        /// </summary>
        /// <value>
        ///     The exact date and time of the next live show.
        /// </value>
        [Display(Name = "Next Show Date/time", Description = "Exact date and time of the next live show")]
        [DateAfterNow(TimeZoneId = "Pacific Standard Time")]
        public DateTime? NextShowDatePst { get; set; }

        /// <summary>
        ///     Gets or sets the show success message.
        /// </summary>
        /// <value>
        ///     The show success message.
        /// </value>
        public bool ShowSuccessMessage => !string.IsNullOrEmpty(this.SuccessMessage);

        /// <summary>
        ///     Gets or sets the success message.
        /// </summary>
        /// <value>
        ///     The success message.
        /// </value>
        public string SuccessMessage { get; set; }

        #endregion
    }
}
