using System;
using System.ComponentModel.DataAnnotations;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Models.Requests;

public sealed record CreateArticleCommentRequest
{
    [Required]
    public required string Content { get; init; }

    public int? ParentCommentId { get; init; }

    public int? QuotedCommentId { get; init; }

    public ArticleComment ToEntity(int userId, int articleId)
    {
        return new ArticleComment
        {
            UserId = userId,
            ArticleId = articleId,
            Content = this.Content,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            IsEdited = false,
            IsDeleted = false,
            ParentCommentId = this.ParentCommentId,
            QuotedCommentId = this.QuotedCommentId
        };
    }
}