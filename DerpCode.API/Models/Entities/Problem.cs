using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class Problem : IIdentifiable<int>
{
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Article ExplanationArticle { get; set; } = default!;

    public int ExplanationArticleId { get; set; }

    public List<Article> SolutionArticles { get; set; } = [];

    [MaxLength(15)]
    public ProblemDifficulty Difficulty { get; set; }

    public List<object> ExpectedOutput { get; set; } = [];

    public List<object> Input { get; set; } = [];

    public List<string> Hints { get; set; } = [];

    public List<Tag> Tags { get; set; } = [];

    public List<ProblemDriver> Drivers { get; set; } = [];

    public List<ProblemSubmission> ProblemSubmissions { get; set; } = [];

    public bool IsPublished { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int LastEditedById { get; set; }

    public User LastEditedBy { get; set; } = default!;

    public int CreatedById { get; set; }

    public User CreatedBy { get; set; } = default!;

    public List<UserFavoriteProblem> FavoritedByUsers { get; set; } = [];
}