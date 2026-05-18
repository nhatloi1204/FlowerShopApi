using AutoMapper;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Enums;
using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace FlowerShop.API.Services.Concrete;

public class ProductService : IProductService
{
    private const string ProductModelType = "Product";
    private const string ProductImageCollection = "gallery";

    private readonly AppDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMediaService _mediaService;
    private readonly IMapper _mapper;

    public ProductService(
        AppDbContext context,
        ISlugService slugService,
        ICloudinaryService cloudinaryService,
        IMediaService mediaService,
        IMapper mapper)
    {
        _context = context;
        _slugService = slugService;
        _cloudinaryService = cloudinaryService;
        _mediaService = mediaService;
        _mapper = mapper;
    }
    public async Task<BaseResponse<PagedList<ProductOutputResource>>> GetProductsAsync(ProductQueryResource query)
    {
        var queryable = _context.Products.AsNoTracking();

        queryable = ApplyFilters(queryable, query);

        var totalItems = await queryable.CountAsync();

        int page = query.Page > 0 ? query.Page : 1;
        int pageSize = query.PageSize > 0 ? query.PageSize : 12;

        // Paginate 
        var products = await queryable
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var resourceItems = await BuildOutputsAsync(products);

        var result = new PagedList<ProductOutputResource>
        {
            Items = resourceItems,
            TotalItems = totalItems,
            CurrentPage = query.Page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };

        return BaseResponse<PagedList<ProductOutputResource>>.Ok(result, "Products retrieved");
    }

    public async Task<BaseResponse<ProductOutputResource>> GetByIdAsync(long id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return BaseResponse<ProductOutputResource>.Fail("Product not found");
        }

        var response = await BuildOutputAsync(product);

        return BaseResponse<ProductOutputResource>.Ok(response, "Product retrieved successfully");
    }

    public async Task<BaseResponse<ProductOutputResource>> GetBySlugAsync(string slug)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);
        if (product == null)
        {
            return BaseResponse<ProductOutputResource>.Fail("Product not found");
        }

        var response = await BuildOutputAsync(product);

        return BaseResponse<ProductOutputResource>.Ok(response, "Product retrieved successfully");
    }

    public async Task<BaseResponse<ProductOutputResource>> CreateAsync(ProductInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductOutputResource>.Fail("Product name is required");
        }

        var slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Products);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = _mapper.Map<Product>(request);
            product.Slug = slug;
            product.Status = request.Status ?? ProductStatus.Available;
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            if (request.CategoryIds != null && request.CategoryIds.Count > 0)
            {
                var categoryIds = request.CategoryIds.Distinct().ToList();
                var categories = await _context.Categories
                    .Where(c => categoryIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync();

                foreach (var categoryId in categories)
                {
                    _context.ProductCategories.Add(new ProductProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId
                    });
                }
            }

            if (request.TagIds != null && request.TagIds.Count > 0)
            {
                var tagIds = request.TagIds.Distinct().ToList();
                var tags = await _context.ProductTags
                    .Where(t => tagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();

                foreach (var tagId in tags)
                {
                    _context.ProductProductTags.Add(new ProductProductTag
                    {
                        ProductId = product.Id,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();

            if (request.Images != null && request.Images.Count > 0)
            {
                await UploadProductImagesAsync(product.Id, request.Images);
            }

            await transaction.CommitAsync();

            var response = await BuildOutputAsync(product);

            return BaseResponse<ProductOutputResource>.Ok(response, "Product created successfully");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BaseResponse<ProductOutputResource>> UpdateAsync(long id, ProductInputResource request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BaseResponse<ProductOutputResource>.Fail("Product name is required");
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return BaseResponse<ProductOutputResource>.Fail("Product not found");
        }

        if (!string.Equals(product.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            product.Slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Products);
        }

        _mapper.Map(request, product);
        if (request.Status.HasValue)
        {
            product.Status = request.Status.Value;
        }
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);

        if (request.CategoryIds != null)
        {
            await UpdateProductCategoriesAsync(product.Id, request.CategoryIds);
        }

        if (request.TagIds != null)
        {
            await UpdateProductTagsAsync(product.Id, request.TagIds);
        }

        await _context.SaveChangesAsync();

        if (request.Images != null && request.Images.Count > 0)
        {
            await ReplaceProductImagesAsync(product.Id, request.Images);
        }

        var response = await BuildOutputAsync(product);

        return BaseResponse<ProductOutputResource>.Ok(response, "Product updated successfully");
    }

    public async Task<BaseResponse<bool>> DeleteAsync(long id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return BaseResponse<bool>.Fail("Product not found");
        }

        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return BaseResponse<bool>.Ok(true, "Product deleted successfully");
    }


    // ------------------- PRIVATE HELPER METHODS -------------------

    private async Task UploadProductImagesAsync(long productId, List<IFormFile> images)
    {
        foreach (var image in images)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(image, "products");
            if (uploadResult == null)
            {
                continue;
            }

            await _mediaService.SaveMediaAsync(uploadResult, image, productId, ProductModelType, ProductImageCollection);
        }
    }

    private async Task ReplaceProductImagesAsync(long productId, List<IFormFile> images)
    {
        var existingMedia = await _context.Medias
            .Where(m => m.ModelType == ProductModelType && m.ModelId == productId)
            .ToListAsync();

        foreach (var media in existingMedia)
        {
            var publicId = ExtractPublicId(media.CustomProperties);
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                await _cloudinaryService.DeleteImageAsync(publicId);
            }
        }

        if (existingMedia.Count > 0)
        {
            _context.Medias.RemoveRange(existingMedia);
            await _context.SaveChangesAsync();
        }

        await UploadProductImagesAsync(productId, images);
    }

    private async Task UpdateProductCategoriesAsync(long productId, List<long> categoryIds)
    {
        var existing = await _context.ProductCategories
            .Where(pc => pc.ProductId == productId)
            .ToListAsync();

        if (existing.Count > 0)
        {
            _context.ProductCategories.RemoveRange(existing);
        }

        if (categoryIds.Count > 0)
        {
            var validIds = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var categoryId in validIds)
            {
                _context.ProductCategories.Add(new ProductProductCategory
                {
                    ProductId = productId,
                    CategoryId = categoryId
                });
            }
        }
    }

    private async Task UpdateProductTagsAsync(long productId, List<long> tagIds)
    {
        var existing = await _context.ProductProductTags
            .Where(pt => pt.ProductId == productId)
            .ToListAsync();

        if (existing.Count > 0)
        {
            _context.ProductProductTags.RemoveRange(existing);
        }

        if (tagIds.Count > 0)
        {
            var validIds = await _context.ProductTags
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            foreach (var tagId in validIds)
            {
                _context.ProductProductTags.Add(new ProductProductTag
                {
                    ProductId = productId,
                    TagId = tagId
                });
            }
        }
    }

    private async Task<ProductOutputResource> BuildOutputAsync(Product product)
    {
        var mediaUrls = await _context.Medias
            .Where(m => m.ModelType == ProductModelType && m.ModelId == product.Id)
            .OrderBy(m => m.OrderColumn)
            .Select(m => m.FileName)
            .ToListAsync();

        var response = _mapper.Map<ProductOutputResource>(product);
        response.ImageUrls = mediaUrls;

        return response;
    }

    private async Task<List<ProductOutputResource>> BuildOutputsAsync(List<Product> products)
    {
        var productIds = products.Select(p => p.Id).ToList();
        var mediaLookup = await _context.Medias
            .Where(m => m.ModelType == ProductModelType && productIds.Contains(m.ModelId))
            .OrderBy(m => m.OrderColumn)
            .ToListAsync();

        var mediaByProduct = mediaLookup
            .GroupBy(m => m.ModelId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.FileName).ToList());

        var responses = _mapper.Map<List<ProductOutputResource>>(products);
        foreach (var response in responses)
        {
            response.ImageUrls = mediaByProduct.TryGetValue(response.Id, out var urls)
                ? urls
                : new List<string>();
        }

        return responses;
    }

    private static string? ExtractPublicId(string customProperties)
    {
        if (string.IsNullOrWhiteSpace(customProperties))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(customProperties);
            if (document.RootElement.TryGetProperty("public_id", out var publicIdElement))
            {
                return publicIdElement.GetString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> queryable, ProductQueryResource query)
    {
        queryable = queryable.Where(p => p.DeletedAt == null);

        if (query.Status.HasValue)
        {
            queryable = queryable.Where(p => p.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var decodedSearch = WebUtility.UrlDecode(query.Search).Trim();
            var normalizedSearch = decodedSearch.ToLower();
            queryable = queryable.Where(p => p.Name!.ToLower().Contains(normalizedSearch) ||
                                             p.Slug.Contains(normalizedSearch));
        }

        if (query.CategoryId.HasValue)
        {
            queryable = queryable.Where(p => p.Categories.Any(pc => pc.Id == query.CategoryId));
        }

        if (query.TagId.HasValue)
        {
            queryable = queryable.Where(p => p.ProductTags.Any(pt => pt.Id == query.TagId));
        }

        // Price filters intentionally commented out for now.
        // if (query.MinPrice.HasValue)
        //     queryable = queryable.Where(p => p.PriceMin >= query.MinPrice);
        // if (query.MaxPrice.HasValue)
        //     queryable = queryable.Where(p => p.PriceMax <= query.MaxPrice);

        return queryable;
    }
}
