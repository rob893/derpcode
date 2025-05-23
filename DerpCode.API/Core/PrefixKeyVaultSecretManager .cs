using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace DerpCode.API.Core;

public sealed class PrefixKeyVaultSecretManager : KeyVaultSecretManager
{
    private readonly IEnumerable<string> prefixes;

    public PrefixKeyVaultSecretManager(IEnumerable<string> prefixes)
    {
        this.prefixes = prefixes ?? throw new ArgumentNullException(nameof(prefixes));
    }

    public override bool Load(SecretProperties secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        return this.prefixes.Any(prefix => secret.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    public override string GetKey(KeyVaultSecret secret)
    {
        ArgumentNullException.ThrowIfNull(secret);

        var prefix = this.prefixes.First(prefix => secret.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

        return secret.Name[$"{prefix}--".Length..].Replace("--", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal);
    }
}
