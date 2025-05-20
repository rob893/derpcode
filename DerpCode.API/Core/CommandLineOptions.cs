using CommandLine;

namespace DerpCode.API.Core;

public sealed class CommandLineOptions
{
    public const string SeedArgument = "seeder";

    public const string DropArgument = "drop";

    public const string SeedDataArgument = "seed";

    public const string MigrateArgument = "migrate";

    public const string ClearDataArgument = "clear";

    [Option("password", Required = false, HelpText = "Input password.")]
    public string? Password { get; set; }
}