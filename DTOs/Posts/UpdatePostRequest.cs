using System.ComponentModel.DataAnnotations;

namespace Posts.DTOs.Posts;

public class UpdatePostRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;
}
