// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;

namespace live.asp.net.Services
{
    /// <summary>
    /// Adds the ASP.NET Core environment name to all tracked telemetry calls.
    /// </summary>
    public class EnvironmentTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHostingEnvironment _hostingEnv;

        public EnvironmentTelemetryInitializer(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnv = hostingEnvironment;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var telemetryWithProperties = telemetry as ISupportProperties;
            if (telemetryWithProperties != null)
            {
                telemetryWithProperties.Properties.Add("Environment name", _hostingEnv.EnvironmentName);
            }
        }
    }
}
