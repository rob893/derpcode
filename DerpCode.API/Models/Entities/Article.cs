using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class Article : IIdentifiable<int>, IOwnedByUser<int>
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;

    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int LastEditedById { get; set; }

    public User LastEditedBy { get; set; } = default!;

    public int UpVotes { get; set; }

    public int DownVotes { get; set; }

    [MaxLength(25)]
    public ArticleType Type { get; set; }

    public List<Tag> Tags { get; set; } = [];

    public bool IsDeleted { get; set; }

    public List<ArticleComment> Comments { get; set; } = [];
}