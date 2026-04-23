using FlowerShop.API.Data;
using FlowerShop.API.Data;
using FlowerShop.API.Helpers;
using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenHelper _jwtHelper;

    public AuthService(AppDbContext context, JwtTokenHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    public async Task<AuthResponse<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Email and password are required"
            };
        }

        if (request.Password != request.PasswordConfirm)
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Passwords do not match"
            };
        }

        // Check if email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Email already exists"
            };
        }

        // Create new user
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Password = PasswordHelper.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate token
        var token = _jwtHelper.GenerateToken(user.Id, user.Email!);
        var expirationMinutes = 60; // from config
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new AuthResponse<LoginResponse>
        {
            Success = true,
            Message = "User registered successfully",
            Data = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Token = token,
                ExpiresAt = expiresAt
            }
        };
    }

    public async Task<AuthResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Email and password are required"
            };
        }

        // Find user
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Verify password
        if (!PasswordHelper.VerifyPassword(request.Password, user.Password!))
        {
            return new AuthResponse<LoginResponse>
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Generate token
        var token = _jwtHelper.GenerateToken(user.Id, user.Email!);
        var expirationMinutes = 60; // from config
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        return new AuthResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                Name = user.Name,
                Token = token,
                ExpiresAt = expiresAt
            }
        };
    }
}
