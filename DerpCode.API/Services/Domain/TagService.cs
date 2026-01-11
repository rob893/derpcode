using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Constants;
using DerpCode.API.Core;
using DerpCode.API.Data.Repositories;
using DerpCode.API.Extensions;
using DerpCode.API.Models.Dtos;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.QueryParameters;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace DerpCode.API.Services.Domain;

/// <summary>
/// Service for managing tag-related business logic
/// </summary>
public sealed class TagService : ITagService
{
    private readonly ITagRepository tagRepository;

    private readonly IMemoryCache cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagService"/> class
    /// </summary>
    /// <param name="tagRepository">The tag repository</param>
    /// <param name="cache">The memory cache</param>
    public TagService(ITagRepository tagRepository, IMemoryCache cache)
    {
        this.tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc />
    public async Task<CursorPaginatedList<TagDto, int>> GetTagsAsync(CursorPaginationQueryParameters searchParams, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(searchParams);

        var tags = await this.GetTagsFromCacheAsync(cancellationToken);

        var tagDtos = tags.Select(TagDto.FromEntity);

        var pagedList = tagDtos.ToCursorPaginatedList(
            item => item.Id,
            item => item.Name,
            CursorConverters.CreateCompositeKeyConverterStringInt(),
            CursorConverters.CreateCompositeCursorConverterStringInt(),
            searchParams,
            true);

        return pagedList;
    }

    /// <inheritdoc />
    public async Task<TagDto?> GetTagByIdAsync(int id, CancellationToken cancellationToken)
    {
        var tags = await this.GetTagsFromCacheAsync(cancellationToken);

        var tag = tags.FirstOrDefault(t => t.Id == id);

        if (tag == null)
        {
            return null;
        }

        return TagDto.FromEntity(tag);
    }

    private async Task<IReadOnlyList<Tag>> GetTagsFromCacheAsync(CancellationToken cancellationToken)
    {
        if (!this.cache.TryGetValue(CacheKeys.Tags, out IReadOnlyList<Tag>? tags))
        {
            // Retrieve all tags from the database
            tags = await this.tagRepository.SearchAsync(_ => true, track: false, cancellationToken);
            this.cache.Set(CacheKeys.Tags, tags, TimeSpan.FromDays(1));
        }

        return tags ?? throw new InvalidOperationException("Failed to retrieve tags from cache.");
    }
}
