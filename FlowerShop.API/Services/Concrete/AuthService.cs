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
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email) ||
                          await _context.Customers.AnyAsync(c => c.Email == request.Email);

        if (emailExists)
            return new AuthResponse<LoginResponse> { Success = false, Message = "Email already exists" };

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Password = PasswordHelper.HashPassword(request.Password!),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password });
    }

    public async Task<AuthResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new AuthResponse<LoginResponse> { Success = false, Message = "Email and password are required" };

        // --- BƯỚC 1: TÌM TRONG BẢNG USERS (ADMIN/STAFF) ---
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user != null && PasswordHelper.VerifyPassword(request.Password, user.Password!))
        {
            var roles = await GetUserRolesAsync(user.Id);
            var permissions = await GetUserPermissionsAsync(user.Id);
            var token = _jwtHelper.GenerateToken(user.Id, user.Email!, roles, permissions);

            return CreateLoginSuccessResponse(user.Id, user.Email!, user.Name!, token);
        }

        // --- BƯỚC 2: TÌM TRONG BẢNG CUSTOMERS (KHÁCH HÀNG) ---
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

        if (customer != null && PasswordHelper.VerifyPassword(request.Password, customer.Password!))
        {
            // Khách hàng không có roles/permissions trong hệ thống quản trị
            var token = _jwtHelper.GenerateToken(customer.Id, customer.Email!, new List<string>(), new List<string>());

            return CreateLoginSuccessResponse(customer.Id, customer.Email!, customer.Name!, token);
        }

        return new AuthResponse<LoginResponse> { Success = false, Message = "Invalid email or password" };
    }

    private AuthResponse<LoginResponse> CreateLoginSuccessResponse(long id, string email, string name, string token)
    {
        return new AuthResponse<LoginResponse>
        {
            Success = true,
            Message = "Login successful",
            Data = new LoginResponse
            {
                UserId = id,
                Email = email,
                Name = name,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // Có thể lấy từ config
            }
        };
    }

    /// <summary>
    /// Get all roles of a user
    /// </summary>
    private async Task<List<string>> GetUserRolesAsync(long userId)
    {
        return await _context.RoleUsers
            .Where(ru => ru.UserId == userId)
            .Include(ru => ru.Role)
            .Select(ru => ru.Role!.Title!)
            .ToListAsync();
    }

    /// <summary>
    /// Get all permissions of a user through their roles
    /// </summary>
    private async Task<List<string>> GetUserPermissionsAsync(long userId)
    {
        return await _context.RoleUsers
            .Where(ru => ru.UserId == userId)
            .Include(ru => ru.Role!)
            .ThenInclude(r => r.Permissions)
            .SelectMany(ru => ru.Role!.Permissions!)
            .Select(p => p.Title!)
            .Distinct()
            .ToListAsync();
    }
}
