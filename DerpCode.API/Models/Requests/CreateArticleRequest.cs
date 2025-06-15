using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateArticleRequest
{
    [Required]
    [MaxLength(255)]
    [MinLength(1)]
    public required string Title { get; init; }

    [Required]
    [MinLength(1)]
    public required string Content { get; init; }

    [Required]
    public required ArticleType Type { get; init; }

    public List<CreateTagRequest> Tags { get; init; } = [];

    public Article ToEntity(int userId)
    {
        return new Article
        {
            UserId = userId,
            Title = this.Title,
            Content = this.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastEditedById = userId,
            Type = this.Type,
            Tags = [.. this.Tags.Select(tag => tag.ToEntity())]
        };
    }
}