using System.ComponentModel.DataAnnotations;

namespace DerpCode.API.Models.Entities;

public sealed class LinkedAccount : IIdentifiable<string>, IOwnedByUser<int>
{
    [MaxLength(255)]
    public string Id { get; set; } = string.Empty;

    [MaxLength(50)]
    public LinkedAccountType LinkedAccountType { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = default!;
}