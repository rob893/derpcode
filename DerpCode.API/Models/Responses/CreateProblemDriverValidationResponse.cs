using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Responses;

public sealed record CreateProblemDriverValidationResponse
{
    public required bool IsValid { get; init; }

    public string? ErrorMessage { get; init; }

    public required LanguageType Language { get; init; }

    public required string Image { get; init; }

    public required SubmissionResult SubmissionResult { get; init; }
}