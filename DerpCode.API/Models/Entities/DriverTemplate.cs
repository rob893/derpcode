using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class DriverTemplate : IIdentifiable<int>
{
    public int Id { get; set; }

    [MaxLength(50)]
    public LanguageType Language { get; set; }

    public string Template { get; set; } = string.Empty;

    public string UITemplate { get; set; } = string.Empty;
}