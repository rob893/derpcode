using System;
using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateProblemDriverRequest
{
    [MinLength(1)]
    [Required]
    public string UITemplate { get; init; } = string.Empty;

    [Required]
    public LanguageType? Language { get; init; }

    [MaxLength(255)]
    [MinLength(1)]
    [Required]
    public string Image { get; init; } = string.Empty;

    [MinLength(1)]
    [Required]
    public string DriverCode { get; init; } = string.Empty;

    [MinLength(1)]
    [Required]
    public string Answer { get; init; } = string.Empty;

    public ProblemDriver ToEntity()
    {
        return new ProblemDriver
        {
            Language = this.Language ?? throw new InvalidOperationException("Language is required"),
            UITemplate = this.UITemplate,
            Image = this.Image,
            DriverCode = this.DriverCode,
            Answer = this.Answer
        };
    }
}