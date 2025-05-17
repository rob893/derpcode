namespace DerpCode.API.Models;

public sealed class ProblemDriver
{
    public string Id { get; set; } = string.Empty;

    public LanguageType Language { get; set; }

    public string Image { get; set; } = string.Empty;

    public string UiTemplate { get; set; } = string.Empty;

    public string DriverCode { get; set; } = string.Empty;
}