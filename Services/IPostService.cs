using Posts.DTOs.Auth;
using Posts.DTOs.Posts;
using Posts.Models.Entities;

namespace Posts.Services;

public interface IPostService
{
    Task<PostResponse> CreateAsync(Guid userId, CreatePostRequest request);
    Task<PagedResult<PostResponse>> GetAllAsync(int page, int pageSize);
    Task<PostResponse> GetByIdAsync(Guid id);
    Task<PostResponse> UpdateAsync(Guid userId, Guid id, UpdatePostRequest request);
    Task DeleteAsync(Guid userId, Guid id);
}
