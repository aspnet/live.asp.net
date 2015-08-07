// --------------------------------------------------------------------------------------------------------------------
// <copyright file="iCalendarOutputFormatter.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Formatters
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using live.asp.net.Models;

    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Net.Http.Headers;

    /// <summary>
    ///     The iCalendar output formatter class.
    /// </summary>
    public class iCalendarOutputFormatter : OutputFormatter
    {
        #region Static Fields

        /// <summary>
        ///     The date time format.
        /// </summary>
        private static readonly string dateTimeFormat = "yyyyMMddTHHmmssZ";

        /// <summary>
        ///     The <see cref="done" /> task.
        /// </summary>
        private static readonly Task done = Task.FromResult(0);

        #endregion

        #region Fields

        /// <summary>
        ///     A value indicating whether there is no next show.
        /// </summary>
        private bool noNextShow;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="iCalendarOutputFormatter"/> class.
        ///     Initializes a new instance of the
        ///     <see cref="iCalendarOutputFormatter"/> class.
        /// </summary>
        public iCalendarOutputFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/calendar"));
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Writes the response body asynchronously.
        /// </summary>
        /// <param name="context">
        /// The <paramref name="context"/> .
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> .
        /// </returns>
        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var liveShowDetails = context.Object as LiveShowDetails;

            Debug.Assert(
                context.Object == null || liveShowDetails != null,
                $"Object to be formatted should be of type {nameof(LiveShowDetails)}");

            if (this.noNextShow)
            {
                return done;
            }

            return WriteLiveShowDetailsToResponseBody(liveShowDetails.NextShowDateUtc, context.HttpContext.Response);
        }

        /// <summary>
        /// Writes the response headers.
        /// </summary>
        /// <param name="context">
        /// The <paramref name="context"/> .
        /// </param>
        public override void WriteResponseHeaders(OutputFormatterContext context)
        {
            var liveShowDetails = context.Object as LiveShowDetails;

            Debug.Assert(
                context.Object == null || liveShowDetails != null,
                $"Object to be formatted should be of type {nameof(LiveShowDetails)}");

            if (liveShowDetails == null || liveShowDetails.NextShowDateUtc == null)
            {
                this.noNextShow = true;
                context.HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                base.WriteResponseHeaders(context);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether <see langword="this"/> instance [can write type]
        ///     the specified declared type.
        /// </summary>
        /// <param name="declaredType">
        /// <see cref="Type"/> of the declared.
        /// </param>
        /// <param name="runtimeType">
        /// <see cref="Type"/> of the runtime.
        /// </param>
        /// <returns>
        /// A value indicating whether <see langword="this"/> instance [can
        ///     write type] the specified declared type.
        /// </returns>
        protected override bool CanWriteType(Type declaredType, Type runtimeType)
            => declaredType == typeof(LiveShowDetails) || runtimeType == typeof(LiveShowDetails);

        /// <summary>
        /// Writes the live show details to <paramref name="response"/> body.
        /// </summary>
        /// <param name="nextShowDateUtc">
        /// The next show date UTC.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <returns>
        /// A Task.
        /// </returns>
        private static Task WriteLiveShowDetailsToResponseBody(DateTime? nextShowDateUtc, HttpResponse response)
        {
            // Internet Calendaring and Scheduling Core Object Specification (iCalendar): https://tools.ietf.org/html/rfc5545
            /* BEGIN:VCALENDAR
               VERSION:2.0
               BEGIN:VEVENT
               UID:aspnet@microsoft.com
               DESCRIPTION:ASP.NET Community Standup
               DTSTART:20150804T170000Z
               DTEND:20150804T035959Z
               LOCATION:https://live.asp.net/
               SUMMARY:ASP.NET Community Standup
               END:VEVENT
               END:VCALENDAR */
            return
                response.WriteAsync(
                    "BEGIN:VCALENDAR\r\n" +
                    "VERSION:2.0\r\n" +
                    "BEGIN:VEVENT\r\n" +
                    "UID:aspnet@microsoft.com\r\n" +
                    "DTSTART:" + nextShowDateUtc?.ToString(dateTimeFormat) + "\r\n" +
                    "DTEND:" + nextShowDateUtc?.AddMinutes(30).ToString(dateTimeFormat) + "\r\n" +
                    "SUMMARY:ASP.NET Community Standup\r\n" +
                    "DESCRIPTION:\r\n" +
                    "LOCATION:https://live.asp.net/\r\n" +
                    "END:VEVENT\r\n" +
                    "END:VCALENDAR\r\n");
        }

        #endregion
    }
}
