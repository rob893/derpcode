using System;
using System.Collections.Generic;

namespace DerpCode.API.Models.Entities;

public sealed class ArticleComment : IIdentifiable<int>, IOwnedByUser<int>
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    public int ArticleId { get; set; }

    public Article Article { get; set; } = default!;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int UpVotes { get; set; }

    public int DownVotes { get; set; }

    public int RepliesCount { get; set; }

    public bool IsEdited { get; set; }

    public bool IsDeleted { get; set; }

    public int? ParentCommentId { get; set; }

    public ArticleComment? ParentComment { get; set; }

    public int? QuotedCommentId { get; set; }

    public ArticleComment? QuotedComment { get; set; }

    public List<ArticleComment> Replies { get; set; } = [];
}