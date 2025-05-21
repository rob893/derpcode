using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record DriverTemplateDto : IIdentifiable<int>
{
    public required int Id { get; init; }

    public required LanguageType Language { get; init; }

    public required string Template { get; init; }

    public required string UITemplate { get; init; }

    public static DriverTemplateDto FromEntity(DriverTemplate driverTemplate)
    {
        ArgumentNullException.ThrowIfNull(driverTemplate);

        return new DriverTemplateDto
        {
            Id = driverTemplate.Id,
            Language = driverTemplate.Language,
            Template = driverTemplate.Template,
            UITemplate = driverTemplate.UITemplate
        };
    }
}
