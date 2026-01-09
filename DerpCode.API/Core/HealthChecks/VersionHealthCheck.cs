using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DerpCode.API.Core.HealthChecks;

public sealed class VersionHealthCheck : IHealthCheck
{
    private readonly Version assemblyVersion;

    private readonly string productVersion;

    public VersionHealthCheck()
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly();
        this.assemblyVersion = entryAssembly?.GetName().Version ?? new Version(0, 0, 0, 0);
        this.productVersion = FileVersionInfo.GetVersionInfo((entryAssembly ?? Assembly.GetExecutingAssembly()).Location).ProductVersion ?? "Unknown ProductVersion";
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var details = new Dictionary<string, object>
        {
            ["assemblyVersion"] = this.assemblyVersion.ToString(),
            ["productVersion"] = this.productVersion
        };
        var result = HealthCheckResult.Healthy($"The current deployed assumbly version is {this.assemblyVersion} and current product version is {this.productVersion}.", details);

        return Task.FromResult(result);
    }
}