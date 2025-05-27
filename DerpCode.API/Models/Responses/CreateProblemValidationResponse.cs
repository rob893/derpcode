using System.Collections.Generic;

namespace DerpCode.API.Models.Responses;

public sealed record CreateProblemValidationResponse
{
    public required bool IsValid { get; init; }

    public string? ErrorMessage { get; init; }

    public required List<CreateProblemDriverValidationResponse> DriverValidations { get; init; } = [];
}