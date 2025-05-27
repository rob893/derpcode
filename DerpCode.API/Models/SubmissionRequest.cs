using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models;

public sealed record SubmissionRequest
{
    public int ProblemId { get; init; }

    [MinLength(1)]
    [Required]
    public string UserCode { get; init; } = string.Empty;

    public LanguageType Language { get; init; }
}