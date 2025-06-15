using System;
using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateProblemExplanationArticleRequest
{
    [Required]
    [MaxLength(255)]
    [MinLength(1)]
    public required string Title { get; init; }

    [Required]
    [MinLength(1)]
    public required string Content { get; init; }

    public Article ToEntity()
    {
        return new Article
        {
            Title = this.Title,
            Content = this.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Type = ArticleType.ProblemSolution
        };
    }

    public static CreateProblemExplanationArticleRequest FromEntity(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new CreateProblemExplanationArticleRequest
        {
            Title = article.Title,
            Content = article.Content
        };
    }
}