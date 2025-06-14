using System;
using System.Security.Claims;
using DerpCode.API.Extensions;
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

    public static ProblemDriverDto FromEntity(ProblemDriver problemDriver, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(problemDriver);
        ArgumentNullException.ThrowIfNull(user);

        var isAdmin = user.IsAdmin();

        return new ProblemDriverDto
        {
            Id = problemDriver.Id,
            ProblemId = problemDriver.ProblemId,
            Language = problemDriver.Language,
            UITemplate = problemDriver.UITemplate,
            Image = isAdmin ? problemDriver.Image : null,
            DriverCode = isAdmin ? problemDriver.DriverCode : null,
            Answer = isAdmin ? problemDriver.Answer : null
        };
    }
}