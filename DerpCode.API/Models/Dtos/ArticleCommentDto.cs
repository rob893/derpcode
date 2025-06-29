using System;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Dtos;

public sealed record ArticleCommentDto : IIdentifiable<int>, IOwnedByUser<int>
{
    public required int Id { get; init; }

    public required int UserId { get; init; }

    public required string UserName { get; init; }

    public required int ArticleId { get; init; }

    public required string Content { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required int UpVotes { get; init; }

    public required int DownVotes { get; init; }

    public required bool IsEdited { get; init; }

    public required bool IsDeleted { get; init; }

    public required int RepliesCount { get; init; }

    public required int? ParentCommentId { get; init; }

    public required int? QuotedCommentId { get; init; }

    public static ArticleCommentDto FromEntity(ArticleComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        return new ArticleCommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = comment.User?.UserName ?? string.Empty,
            ArticleId = comment.ArticleId,
            Content = comment.Content,
            UpVotes = comment.UpVotes,
            RepliesCount = comment.RepliesCount,
            DownVotes = comment.DownVotes,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            IsEdited = comment.IsEdited,
            IsDeleted = comment.IsDeleted,
            ParentCommentId = comment.ParentCommentId,
            QuotedCommentId = comment.QuotedCommentId
        };
    }
}