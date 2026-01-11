using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record UserFavoriteProblemDto
{
    public required int UserId { get; init; }

    public required int ProblemId { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public static UserFavoriteProblemDto FromEntity(UserFavoriteProblem userFavoriteProblem)
    {
        ArgumentNullException.ThrowIfNull(userFavoriteProblem);

        return new UserFavoriteProblemDto
        {
            UserId = userFavoriteProblem.UserId,
            ProblemId = userFavoriteProblem.ProblemId,
            CreatedAt = userFavoriteProblem.CreatedAt
        };
    }
}