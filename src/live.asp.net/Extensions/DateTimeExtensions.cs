// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System
{
    public static class DateTimeExtensions
    {
        private const string PST = "Pacific Standard Time";
        private static readonly TimeZoneInfo _pstTimeZone = TimeZoneInfo.FindSystemTimeZoneById(PST);

        public static DateTime ConvertToTimeZone(this DateTime dateTime, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            return TimeZoneInfo.ConvertTime(dateTime, sourceTimeZone, destinationTimeZone);
        }

        public static DateTime ConvertFromUtcToPst(this DateTime dateTime)
        {
            return dateTime.ConvertToTimeZone(TimeZoneInfo.Utc, _pstTimeZone);
        }

        public static DateTime ConvertFromPtcToUtc(this DateTime dateTime)
        {
            return dateTime.ConvertToTimeZone(_pstTimeZone, TimeZoneInfo.Utc);
        }
    }
}
