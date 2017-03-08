// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace live.asp.net.Services
{
    public class DeploymentEnvironment : IDeploymentEnvironment
    {
        private readonly string _contentRoot;
        private readonly ILogger<DeploymentEnvironment> _logger;
        private string _commitSha;
        private string _runtimeFramework;

        public DeploymentEnvironment(IHostingEnvironment hostingEnv, ILogger<DeploymentEnvironment> logger)
        {
            _contentRoot = hostingEnv.ContentRootPath;
            _logger = logger;
        }

        public string DeploymentId
        {
            get
            {
                if (_commitSha == null)
                {
                    LoadCommitSha();
                }

                return _commitSha;
            }
        }

        public string RuntimeFramework
        {
            get
            {
                if (_runtimeFramework == null)
                {
                    LoadRuntimeFramework();
                }

                return _runtimeFramework;
            }
        }

        private void LoadCommitSha()
        {
            var kuduActiveDeploymentPath = Path.GetFullPath(Path.Combine(_contentRoot, "..", "deployments", "active"));
            try
            {
                if (File.Exists(kuduActiveDeploymentPath))
                {
                    _logger.LogDebug("Kudu active deployment file found, using it to set DeploymentID");
                    _commitSha = File.ReadAllText(kuduActiveDeploymentPath) + " (kudu)";
                }
                else
                {
                    _logger.LogDebug("Kudu active deployment file not found, using git to set DeploymentID");
                    var git = Process.Start(new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = "rev-parse HEAD",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                    var gitOut = "";
                    while (!git.StandardOutput.EndOfStream)
                    {
                        gitOut += git.StandardOutput.ReadLine();
                    }
                    gitOut += " (local)";

                    git.WaitForExit();
                    if (git.ExitCode != 0)
                    {
                        _logger.LogDebug("Problem using git to set deployment ID:\r\n  git exit code: {0}\r\n git output: {1}", git.ExitCode, _commitSha);
                        _commitSha = "(Could not determine deployment ID)";
                    }
                    else
                    {
                        _commitSha = gitOut;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Error determining deployment ID");
                _commitSha = "(Error determining deployment ID)";
            }
        }

        private void LoadRuntimeFramework()
        {
            var framework = GetFrameworkName();
            var arch = RuntimeInformation.ProcessArchitecture;
            var version = GetFrameworkVersion();

            _runtimeFramework = $"{framework} {version}, {arch}";
        }

        private static string GetFrameworkName()
        {
            var description = RuntimeInformation.FrameworkDescription;

            var indexOfFirstDigit = -1;
            for (int i = 0; i < description.Length; i++)
            {
                if (Char.IsDigit(description[i]))
                {
                    indexOfFirstDigit = i;
                    break;
                }
            }

            if (indexOfFirstDigit > 0)
            {
                description = description.Substring(0, indexOfFirstDigit).Trim();
            }

            return description;
        }

        private string GetFrameworkVersion()
        {
            var fxDepsFile = AppContext.GetData("FX_DEPS_FILE") as string;

            if (!string.IsNullOrEmpty(fxDepsFile))
            {
                try
                {
                    var frameworkDir = new FileInfo(fxDepsFile) // Microsoft.NETCore.App.deps.json
                        .Directory; // (version)

                    if (Version.TryParse(frameworkDir.Name, out Version frameworkVersion))
                    {
                        return frameworkVersion.ToString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, "Error determining framework version");
                }
            }

            return PlatformServices.Default.Application.RuntimeFramework.Version.ToString();
        }
    }
}
