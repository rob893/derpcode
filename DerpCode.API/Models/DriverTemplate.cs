namespace DerpCode.API.Models;

public sealed class DriverTemplate
{
    public string Id { get; set; } = string.Empty;

    public LanguageType Language { get; set; }

    public string Template { get; set; } = string.Empty;

    public string UiTemplate { get; set; } = string.Empty;
}
