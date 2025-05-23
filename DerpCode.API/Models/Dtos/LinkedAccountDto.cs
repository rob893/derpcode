using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public record LinkedAccountDto : IIdentifiable<string>, IOwnedByUser<int>
{
    public required string Id { get; init; }

    public required LinkedAccountType LinkedAccountType { get; init; }

    public required int UserId { get; init; }

    public static LinkedAccountDto FromEntity(LinkedAccount linkedAccount)
    {
        ArgumentNullException.ThrowIfNull(linkedAccount);

        return new LinkedAccountDto
        {
            Id = linkedAccount.Id,
            LinkedAccountType = linkedAccount.LinkedAccountType,
            UserId = linkedAccount.UserId
        };
    }
}