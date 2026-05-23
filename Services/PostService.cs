using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Posts.DTOs.Posts;
using Posts.Models.Entities;
using Posts.Repositories;

namespace Posts.Services;

public class PostService : IPostService
{
    private const string PostsCacheVersionKey = "posts:all:version";

    private static readonly JsonSerializerOptions CacheJsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly DistributedCacheEntryOptions PostsCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private static readonly DistributedCacheEntryOptions PostsCacheVersionOptions = new()
    {
        SlidingExpiration = TimeSpan.FromDays(7)
    };

    private readonly IPostRepository _postRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<PostService> _logger;

    public PostService(
        IPostRepository postRepository,
        IDistributedCache cache,
        ILogger<PostService> logger)
    {
        _postRepository = postRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PostResponse> CreateAsync(Guid userId, CreatePostRequest request)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        await _postRepository.AddAsync(post);
        await InvalidatePostsCacheAsync();

        _logger.LogInformation("Post created: {PostId} by user {UserId}", post.Id, userId);

        return MapToResponse(post);
    }

    public async Task<PagedResult<PostResponse>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var cacheVersion = await GetPostsCacheVersionAsync();
        var cacheKey = GetPostsCacheKey(cacheVersion, page, pageSize);
        var cachedResult = await TryGetCachedPostsAsync(cacheKey);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        var items = await _postRepository.GetPagedAsync(
            null,
            page,
            pageSize,
            q => q.OrderByDescending(p => p.CreatedAt));

        var total = await _postRepository.CountAsync();

        var result = new PagedResult<PostResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };

        await TrySetCachedPostsAsync(cacheKey, result);

        return result;
    }

    public async Task<PostResponse> GetByIdAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Post with ID {id} not found.");

        return MapToResponse(post);
    }

    public async Task<PostResponse> UpdateAsync(Guid userId, Guid id, UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Post with ID {id} not found.");

        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own posts.");
        }

        post.Title = request.Title;
        post.Description = request.Description;

        await _postRepository.UpdateAsync(post);
        await InvalidatePostsCacheAsync();

        _logger.LogInformation("Post updated: {PostId} by user {UserId}", id, userId);

        return MapToResponse(post);
    }

    public async Task DeleteAsync(Guid userId, Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Post with ID {id} not found.");

        if (post.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own posts.");
        }

        await _postRepository.DeleteAsync(post);
        await InvalidatePostsCacheAsync();

        _logger.LogInformation("Post deleted: {PostId} by user {UserId}", id, userId);
    }

    private async Task<string> GetPostsCacheVersionAsync()
    {
        try
        {
            var version = await _cache.GetStringAsync(PostsCacheVersionKey);

            if (!string.IsNullOrWhiteSpace(version))
            {
                return version;
            }

            version = Guid.NewGuid().ToString("N");
            await _cache.SetStringAsync(PostsCacheVersionKey, version, PostsCacheVersionOptions);

            return version;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read posts cache version. Falling back to uncached posts query.");
            return Guid.NewGuid().ToString("N");
        }
    }

    private async Task<PagedResult<PostResponse>?> TryGetCachedPostsAsync(string cacheKey)
    {
        try
        {
            var cachedPosts = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrWhiteSpace(cachedPosts))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PagedResult<PostResponse>>(cachedPosts, CacheJsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read posts from cache for key {CacheKey}.", cacheKey);
            return null;
        }
    }

    private async Task TrySetCachedPostsAsync(string cacheKey, PagedResult<PostResponse> posts)
    {
        try
        {
            var serializedPosts = JsonSerializer.Serialize(posts, CacheJsonOptions);
            await _cache.SetStringAsync(cacheKey, serializedPosts, PostsCacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not cache posts for key {CacheKey}.", cacheKey);
        }
    }

    private async Task InvalidatePostsCacheAsync()
    {
        try
        {
            var version = Guid.NewGuid().ToString("N");
            await _cache.SetStringAsync(PostsCacheVersionKey, version, PostsCacheVersionOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not invalidate posts cache.");
        }
    }

    private static string GetPostsCacheKey(string cacheVersion, int page, int pageSize)
    {
        return $"posts:all:{cacheVersion}:page:{page}:size:{pageSize}";
    }

    private static PostResponse MapToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            CreatedAt = post.CreatedAt,
            UserId = post.UserId
        };
    }
}
