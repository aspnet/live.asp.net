// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;

namespace live.asp.net.Formatters
{
    public class iCalendarOutputFormatter : OutputFormatter
    {
        public iCalendarOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/calendar"));
        }

        protected override bool CanWriteType(Type declaredType, Type runtimeType)
        {
            return runtimeType == typeof(LiveShowDetails);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var liveShowDetails = context.Object as LiveShowDetails;

            Debug.Assert(liveShowDetails != null, $"Object to be formatted should be of type {nameof(LiveShowDetails)}");

            return WriteLiveShowDetailsToResponseBody(liveShowDetails, context.HttpContext.Response.Body);
        }

        private static string _iCalHeader =
            "BEGIN:VCALENDAR" + Environment.NewLine +
            "VERSION:2.0" + Environment.NewLine +
            "BEGIN:VEVENT" + Environment.NewLine +
            "UID:aspnet@microsoft.com" + Environment.NewLine +
            "DTSTART:";

        private static string _iCalMiddle = Environment.NewLine + "DTEND:";

        private static string _iCalFooter =
            Environment.NewLine +
            "SUMMARY:ASP.NET Community Standup" + Environment.NewLine +
            "DESCRIPTION:" + Environment.NewLine +
            "LOCATION:https://live.asp.net/" + Environment.NewLine +
            "END:VEVENT" + Environment.NewLine +
            "END:VCALENDAR";

        private static byte[] _iCalHeaderBytes = Encoding.UTF8.GetBytes(_iCalHeader);
        private static byte[] _iCalMiddleBytes = Encoding.UTF8.GetBytes(_iCalMiddle);
        private static byte[] _iCalFooterBytes = Encoding.UTF8.GetBytes(_iCalFooter);

        private static string _dateTimeFormat = "yyyyMMddTHHmmssZ";

        private static int _footerOffset =
            _iCalHeaderBytes.Length +
            Encoding.UTF8.GetByteCount(_dateTimeFormat + _iCalMiddle + _dateTimeFormat);

        private static int _payloadLength = _footerOffset + _iCalFooterBytes.Length;

        private static Task WriteLiveShowDetailsToResponseBody(LiveShowDetails liveShowDetails, Stream body)
        {
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

            var start = Encoding.UTF8.GetBytes(liveShowDetails.NextShowDateUtc.Value.ToString(_dateTimeFormat));
            var end = Encoding.UTF8.GetBytes(liveShowDetails.NextShowDateUtc.Value.AddMinutes(30).ToString(_dateTimeFormat));

            var responseBuffer = new byte[_payloadLength];

            Buffer.BlockCopy(_iCalHeaderBytes, 0, responseBuffer, 0, _iCalHeaderBytes.Length);
            Buffer.BlockCopy(start, 0, responseBuffer, _iCalHeaderBytes.Length, start.Length);
            Buffer.BlockCopy(_iCalMiddleBytes, 0, responseBuffer, _iCalHeaderBytes.Length + start.Length, _iCalMiddleBytes.Length);
            Buffer.BlockCopy(end, 0, responseBuffer, _iCalHeaderBytes.Length + start.Length + _iCalMiddleBytes.Length + end.Length, end.Length);
            Buffer.BlockCopy(_iCalFooterBytes, 0, responseBuffer, _footerOffset, _iCalFooterBytes.Length);

            return body.WriteAsync(responseBuffer, 0, responseBuffer.Length);
        }
    }
}
