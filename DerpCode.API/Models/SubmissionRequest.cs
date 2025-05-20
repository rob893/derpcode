using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models;

public sealed record SubmissionRequest
{
    public string UserCode { get; init; } = string.Empty;

    public LanguageType Language { get; init; }
}
