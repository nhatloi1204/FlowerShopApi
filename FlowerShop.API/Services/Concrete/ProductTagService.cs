using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Implementations;

public class ProductTagService : IProductTagService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ProductTagService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // 1. LẤY TẤT CẢ TAG (Đã có Global Filter ở Entity tự loại bản ghi xóa mềm)
    public async Task<BaseResponse<IEnumerable<ProductTagOutputResource>>> GetAllAsync()
    {
        var tags = await _context.ProductTags
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var response = _mapper.Map<IEnumerable<ProductTagOutputResource>>(tags);
        return BaseResponse<IEnumerable<ProductTagOutputResource>>.Ok(response, "Fetched all tags successfully");
    }

    // 2. LẤY CHI TIẾT THEO ID
    public async Task<BaseResponse<ProductTagOutputResource>> GetByIdAsync(long id)
    {
        var tag = await _context.ProductTags.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        if (tag == null)
        {
            return BaseResponse<ProductTagOutputResource>.Fail("Tag not found");
        }

        var response = _mapper.Map<ProductTagOutputResource>(tag);
        return BaseResponse<ProductTagOutputResource>.Ok(response, "Fetched tag successfully");
    }

    // 3. TẠO TAG MỚI (Vá lỗ hổng trùng tên)
    public async Task<BaseResponse<ProductTagOutputResource>> CreateAsync(ProductTagInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductTagOutputResource>.Fail("Tag name is required");
        }

        // 🎯 KIỂM TRA TRÙNG TÊN: Tránh lỗi Unique Constraint dính vào bản ghi chưa bị xóa mềm
        var nameExists = await _context.ProductTags
            .AnyAsync(t => t.Name!.ToLower() == request.Name.Trim().ToLower() && t.DeletedAt == null);

        if (nameExists)
        {
            return BaseResponse<ProductTagOutputResource>.Fail($"Tag with name '{request.Name}' already exists.");
        }

        var tag = _mapper.Map<ProductTag>(request);
        tag.Name = request.Name.Trim();
        tag.CreatedAt = DateTime.UtcNow;
        tag.UpdatedAt = DateTime.UtcNow;

        await _context.ProductTags.AddAsync(tag);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ProductTagOutputResource>(tag);
        return BaseResponse<ProductTagOutputResource>.Ok(response, "Tag created successfully");
    }

    // 4. CẬP NHẬT TAG (Fix lỗi thứ tự Mapping giống Category)
    public async Task<BaseResponse<ProductTagOutputResource>> UpdateAsync(long id, ProductTagInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductTagOutputResource>.Fail("Tag name is required");
        }

        var tag = await _context.ProductTags.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        if (tag == null)
        {
            return BaseResponse<ProductTagOutputResource>.Fail("Tag not found");
        }

        // 🎯 KIỂM TRA TRÙNG TÊN: Nếu đổi sang tên mới, kiểm tra xem tên mới đó đã có tag nào khác chiếm chưa
        var cleanNewName = request.Name.Trim();
        if (!string.Equals(tag.Name, cleanNewName, StringComparison.OrdinalIgnoreCase))
        {
            var nameExists = await _context.ProductTags
                .AnyAsync(t => t.Id != id && t.Name!.ToLower() == cleanNewName.ToLower() && t.DeletedAt == null);

            if (nameExists)
            {
                return BaseResponse<ProductTagOutputResource>.Fail($"Another tag with name '{request.Name}' already exists.");
            }
        }

        // Thực hiện gán dữ liệu mượt mà
        _mapper.Map(request, tag);
        tag.Name = cleanNewName;
        tag.UpdatedAt = DateTime.UtcNow;

        _context.ProductTags.Update(tag);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<ProductTagOutputResource>(tag);
        return BaseResponse<ProductTagOutputResource>.Ok(response, "Tag updated successfully");
    }

    // 5. XÓA MỀM TAG (Vá lỗ hổng Cascade Delete giả)
    public async Task<BaseResponse<bool>> DeleteAsync(long id)
    {
        var tag = await _context.ProductTags.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
        if (tag == null)
        {
            return BaseResponse<bool>.Fail("Tag not found");
        }

        // 🎯 VÁ LỖ HỔNG CASCADE DELETE: Do ta dùng Soft Delete nên DB không tự chạy Cascade!
        // Ta phải chủ động dọn sạch liên kết của Tag này với các Product ở bảng trung gian.
        var intermediateLinks = _context.Set<ProductProductTag>().Where(ppt => ppt.TagId == id);
        _context.Set<ProductProductTag>().RemoveRange(intermediateLinks);

        // Đóng dấu xóa mềm cho bản ghi chính
        tag.DeletedAt = DateTime.UtcNow;
        tag.UpdatedAt = DateTime.UtcNow;

        _context.ProductTags.Update(tag);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Tag deleted successfully and unlinked from all products.");
    }
}