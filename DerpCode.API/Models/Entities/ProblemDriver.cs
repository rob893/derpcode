using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class ProblemDriver : IIdentifiable<int>
{
    public int Id { get; set; }

    public int ProblemId { get; set; }

    public Problem Problem { get; set; } = default!;

    [MaxLength(50)]
    public LanguageType Language { get; set; }

    [MaxLength(255)]
    public string Image { get; set; } = string.Empty;

    public string UITemplate { get; set; } = string.Empty;

    public string DriverCode { get; set; } = string.Empty;
}