using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ProblemDriverDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required int ProblemId { get; init; }

    public required LanguageType Language { get; init; }

    public required string UITemplate { get; init; }

    public required string? Image { get; init; }

    public required string? DriverCode { get; init; }

    public required string? Answer { get; init; }

    public static ProblemDriverDto FromEntity(ProblemDriver problemDriver, bool isCurrentUserAdmin)
    {
        ArgumentNullException.ThrowIfNull(problemDriver);

        return new ProblemDriverDto
        {
            Id = problemDriver.Id,
            ProblemId = problemDriver.ProblemId,
            Language = problemDriver.Language,
            UITemplate = problemDriver.UITemplate,
            Image = isCurrentUserAdmin ? problemDriver.Image : null,
            DriverCode = isCurrentUserAdmin ? problemDriver.DriverCode : null,
            Answer = isCurrentUserAdmin ? problemDriver.Answer : null
        };
    }
}