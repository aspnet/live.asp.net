// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace live.asp.net
{
    public static class Timing
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        public static long GetTimestamp() => Stopwatch.GetTimestamp();

        public static TimeSpan GetDuration(long start) => GetDuration(start, Stopwatch.GetTimestamp());

        public static TimeSpan GetDuration(long start, long end) => new TimeSpan((long)(TimestampToTicks * (end - start)));
    }
}
