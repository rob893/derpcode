using System;
using System.Collections.Generic;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ArticleDto : IIdentifiable<int>, IOwnedByUser<int>
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required int UpVotes { get; init; }

    public required int DownVotes { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required int LastEditedById { get; init; }

    public required ArticleType Type { get; init; }

    public required IReadOnlyList<TagDto> Tags { get; init; }

    public static ArticleDto FromEntity(Article article)
    {
        ArgumentNullException.ThrowIfNull(article);

        return new ArticleDto
        {
            Id = article.Id,
            UserId = article.UserId,
            Title = article.Title,
            Content = article.Content,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            UpVotes = article.UpVotes,
            DownVotes = article.DownVotes,
            LastEditedById = article.LastEditedById,
            Type = article.Type,
            Tags = article.Tags.ConvertAll(TagDto.FromEntity)
        };
    }
}