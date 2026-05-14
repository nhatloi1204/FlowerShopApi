using FlowerShop.API.Data;
using AutoMapper;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PermissionService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<BaseResponse<List<PermissionOutputResource>>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Title)
            .ToListAsync();

        return new BaseResponse<List<PermissionOutputResource>>
        {
            Success = true,
            Message = "Permissions retrieved successfully",
            Data = _mapper.Map<List<PermissionOutputResource>>(permissions)
        };
    }

    public async Task<BaseResponse<PermissionOutputResource>> GetPermissionByIdAsync(long permissionId)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == permissionId);

        if (permission == null)
        {
            return BaseResponse<PermissionOutputResource>.Fail("Permission not found");
        }

        return new BaseResponse<PermissionOutputResource>
        {
            Success = true,
            Message = "Permission retrieved successfully",
            Data = _mapper.Map<PermissionOutputResource>(permission)
        };
    }
}
