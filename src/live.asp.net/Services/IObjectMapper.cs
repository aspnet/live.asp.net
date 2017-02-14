// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace live.asp.net.Services
{
    public interface IObjectMapper
    {
        TDest Map<TSource, TDest>(TSource source, TDest dest);
    }
}
