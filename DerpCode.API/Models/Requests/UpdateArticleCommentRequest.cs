using System;
using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record UpdateArticleCommentRequest
{
    [Required]
    public required string Content { get; init; }

    public ArticleComment ToEntity(ArticleComment existingComment)
    {
        ArgumentNullException.ThrowIfNull(existingComment);

        return new ArticleComment
        {
            Id = existingComment.Id,
            UserId = existingComment.UserId,
            ArticleId = existingComment.ArticleId,
            Content = this.Content,
            CreatedAt = existingComment.CreatedAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsEdited = true,
            IsDeleted = existingComment.IsDeleted,
            ParentCommentId = existingComment.ParentCommentId,
            QuotedCommentId = existingComment.QuotedCommentId
        };
    }
}