using Posts.Models.Entities;

namespace Posts.Repositories;

public interface IPostRepository : IRepository<Post>
{
    Task<IReadOnlyList<Post>> GetByUserIdAsync(Guid userId);
}
