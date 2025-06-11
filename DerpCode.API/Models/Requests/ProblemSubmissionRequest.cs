using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record ProblemSubmissionRequest
{
    [MinLength(1)]
    [Required]
    public string UserCode { get; init; } = string.Empty;

    public LanguageType Language { get; init; }
}