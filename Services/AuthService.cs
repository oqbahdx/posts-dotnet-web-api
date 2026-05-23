using Posts.DTOs.Auth;
using Posts.Helpers;
using Posts.Models.Entities;
using Posts.Repositories;

namespace Posts.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var token = _jwtService.GenerateToken(user);
        var expirationMinutes = int.Parse(
            System.Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60");

        _logger.LogInformation("User registered successfully: {Email}", user.Email);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var token = _jwtService.GenerateToken(user);
        var expirationMinutes = int.Parse(
            System.Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES") ?? "60");

        _logger.LogInformation("User logged in successfully: {Email}", user.Email);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }
}
