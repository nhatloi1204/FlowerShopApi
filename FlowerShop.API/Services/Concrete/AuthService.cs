using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Helpers;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtTokenHelper _jwtHelper;
    private readonly IMapper _mapper;
    private readonly IConfiguration configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthService(
        AppDbContext context,
        JwtTokenHelper jwtHelper,
        IMapper mapper,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _mapper = mapper;
        this.configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<BaseResponse<LoginOutputResource>> RegisterAsync(RegisterInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BaseResponse<LoginOutputResource>.Fail("Email and password are required");
        }

        if (request.Password != request.PasswordConfirm)
        {
            return BaseResponse<LoginOutputResource>.Fail("Passwords do not match");
        }

        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email) ||
                          await _context.Customers.AnyAsync(c => c.Email == request.Email);

        if (emailExists)
        {
            return BaseResponse<LoginOutputResource>.Fail("Email already exists");
        }

        var customer = _mapper.Map<Customer>(request);
        customer.Password = PasswordHelper.HashPassword(request.Password!);
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return await LoginAsync(new LoginInputResource { Email = request.Email, Password = request.Password });
    }

    public async Task<BaseResponse<LoginOutputResource>> LoginAsync(LoginInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BaseResponse<LoginOutputResource>.Fail("Email and password are required");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user != null && PasswordHelper.VerifyPassword(request.Password, user.Password!))
        {
            var roles = await GetUserRolesAsync(user.Id);
            var permissions = await GetUserPermissionsAsync(user.Id);
            var token = _jwtHelper.GenerateToken(user.Id, user.Email!, roles, permissions);

            return CreateLoginSuccessResponse(user, token);
        }

        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == request.Email);

        if (customer != null && PasswordHelper.VerifyPassword(request.Password, customer.Password!))
        {
            var token = _jwtHelper.GenerateToken(customer.Id, customer.Email!, new List<string>(), new List<string>());

            return CreateLoginSuccessResponse(customer, token);
        }

        return BaseResponse<LoginOutputResource>.Fail("Invalid email or password");
    }

    public async Task<BaseResponse<LoginOutputResource>> ExternalLoginAsync(ExternalAuthInputResource request)
    {
        if (request.Provider.ToUpper() == "GOOGLE")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AccessToken))
                {
                    return BaseResponse<LoginOutputResource>.Fail("Access token is required");
                }

                var httpClient = _httpClientFactory.CreateClient();
                var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
                userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

                var userInfoResponse = await httpClient.SendAsync(userInfoRequest);
                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    return BaseResponse<LoginOutputResource>.Fail("Invalid External Token");
                }

                var payload = await userInfoResponse.Content.ReadFromJsonAsync<GoogleUserInfoResource>();
                if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
                {
                    return BaseResponse<LoginOutputResource>.Fail("Invalid External Token");
                }

                var customer = await _context.Customers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Email == payload.Email ||
                                             (c.Provider == "GOOGLE" && c.ProviderAccountId == payload.Sub));

                if (customer == null)
                {
                    customer = _mapper.Map<Customer>(payload);
                    _context.Customers.Add(customer);
                }
                else
                {
                    _mapper.Map(payload, customer);
                    customer.DeletedAt = null;
                }

                await _context.SaveChangesAsync();

                var token = _jwtHelper.GenerateToken(customer.Id, customer.Email!, new List<string>(), new List<string>());
                return CreateLoginSuccessResponse(customer, token);
            }
            catch (Exception)
            {
                return BaseResponse<LoginOutputResource>.Fail("Invalid External Token");
            }
        }

        return BaseResponse<LoginOutputResource>.Fail("Unsupported Provider");
    }

    private BaseResponse<LoginOutputResource> CreateLoginSuccessResponse(User user, string token)
    {
        var response = _mapper.Map<LoginOutputResource>(user);
        response.Token = token;
        response.ExpiresAt = DateTime.UtcNow.AddMinutes(60);

        return BaseResponse<LoginOutputResource>.Ok(response, "Login successful");
    }

    private BaseResponse<LoginOutputResource> CreateLoginSuccessResponse(Customer customer, string token)
    {
        var response = _mapper.Map<LoginOutputResource>(customer);
        response.Token = token;
        response.ExpiresAt = DateTime.UtcNow.AddMinutes(60);

        return BaseResponse<LoginOutputResource>.Ok(response, "Login successful");
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
