// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Options;

namespace live.asp.net.Services
{
    public class ApplicationInsightsServiceOptionsSetup : IConfigureOptions<ApplicationInsightsServiceOptions>
    {
        private readonly IDeploymentEnvironment _deploymentEnvironment;

        public ApplicationInsightsServiceOptionsSetup(IDeploymentEnvironment deploymentEnvironment)
        {
            _deploymentEnvironment = deploymentEnvironment;
        }

        public void Configure(ApplicationInsightsServiceOptions options)
        {
            options.ApplicationVersion = _deploymentEnvironment.DeploymentId.Substring(0, 7);
        }
    }
}
