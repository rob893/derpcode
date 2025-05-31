using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record AdminProblemDriverDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required int ProblemId { get; init; }

    public required LanguageType Language { get; init; }

    public required string UITemplate { get; init; }

    public required string Image { get; init; }

    public required string DriverCode { get; init; }

    public required string Answer { get; init; }

    public static AdminProblemDriverDto FromEntity(ProblemDriver problemDriver)
    {
        ArgumentNullException.ThrowIfNull(problemDriver);

        return new AdminProblemDriverDto
        {
            Id = problemDriver.Id,
            ProblemId = problemDriver.ProblemId,
            Language = problemDriver.Language,
            UITemplate = problemDriver.UITemplate,
            Image = problemDriver.Image,
            DriverCode = problemDriver.DriverCode,
            Answer = problemDriver.Answer
        };
    }
}