using System.Text.Json;
using FlowerShop.API.Data;
using FlowerShop.API.Models.DTOs.Auth;
using FlowerShop.API.Models.DTOs.Product;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Enums;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class ProductService : IProductService
{
    private const string ProductModelType = "Product";
    private const string ProductImageCollection = "gallery";

    private readonly AppDbContext _context;
    private readonly ISlugService _slugService;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IMediaService _mediaService;

    public ProductService(
        AppDbContext context,
        ISlugService slugService,
        ICloudinaryService cloudinaryService,
        IMediaService mediaService)
    {
        _context = context;
        _slugService = slugService;
        _cloudinaryService = cloudinaryService;
        _mediaService = mediaService;
    }

    public async Task<AuthResponse<ProductResponse>> CreateAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new AuthResponse<ProductResponse>
            {
                Success = false,
                Message = "Product name is required"
            };
        }

        var slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Products);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = new Product
            {
                Name = request.Name,
                Slug = slug,
                PriceMin = request.PriceMin,
                PriceMax = request.PriceMax,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Description = request.Description,
                Status = request.Status ?? ProductStatus.Available,
                CompanyId = request.CompanyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

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

            var response = await MapToResponseAsync(product);

            return new AuthResponse<ProductResponse>
            {
                Success = true,
                Message = "Product created successfully",
                Data = response
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<AuthResponse<ProductResponse>> UpdateAsync(long id, UpdateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return new AuthResponse<ProductResponse>
            {
                Success = false,
                Message = "Product name is required"
            };
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return new AuthResponse<ProductResponse>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        if (!string.Equals(product.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            product.Slug = await _slugService.GenerateUniqueSlugAsync(request.Name, _context.Products);
        }

        product.Name = request.Name;
        product.PriceMin = request.PriceMin;
        product.PriceMax = request.PriceMax;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.Description = request.Description;
        if (request.Status.HasValue)
        {
            product.Status = request.Status.Value;
        }
        product.CompanyId = request.CompanyId;
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

        var response = await MapToResponseAsync(product);

        return new AuthResponse<ProductResponse>
        {
            Success = true,
            Message = "Product updated successfully",
            Data = response
        };
    }

    public async Task<AuthResponse<bool>> DeleteAsync(long id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return new AuthResponse<bool>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        product.DeletedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return new AuthResponse<bool>
        {
            Success = true,
            Message = "Product deleted successfully",
            Data = true
        };
    }

    public async Task<AuthResponse<ProductResponse>> GetByIdAsync(long id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return new AuthResponse<ProductResponse>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        var response = await MapToResponseAsync(product);

        return new AuthResponse<ProductResponse>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = response
        };
    }

    public async Task<AuthResponse<ProductResponse>> GetBySlugAsync(string slug)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Slug == slug);
        if (product == null)
        {
            return new AuthResponse<ProductResponse>
            {
                Success = false,
                Message = "Product not found"
            };
        }

        var response = await MapToResponseAsync(product);

        return new AuthResponse<ProductResponse>
        {
            Success = true,
            Message = "Product retrieved successfully",
            Data = response
        };
    }

    public async Task<AuthResponse<List<ProductResponse>>> GetAllAsync()
    {
        var products = await _context.Products
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var responses = await MapToResponsesAsync(products);

        return new AuthResponse<List<ProductResponse>>
        {
            Success = true,
            Message = "Products retrieved successfully",
            Data = responses
        };
    }

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

    private async Task<ProductResponse> MapToResponseAsync(Product product)
    {
        var mediaUrls = await _context.Medias
            .Where(m => m.ModelType == ProductModelType && m.ModelId == product.Id)
            .OrderBy(m => m.OrderColumn)
            .Select(m => m.FileName)
            .ToListAsync();

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            PriceMin = product.PriceMin,
            PriceMax = product.PriceMax,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description,
            Status = product.Status,
            CompanyId = product.CompanyId,
            ImageUrls = mediaUrls,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private async Task<List<ProductResponse>> MapToResponsesAsync(List<Product> products)
    {
        var productIds = products.Select(p => p.Id).ToList();
        var mediaLookup = await _context.Medias
            .Where(m => m.ModelType == ProductModelType && productIds.Contains(m.ModelId))
            .OrderBy(m => m.OrderColumn)
            .ToListAsync();

        var mediaByProduct = mediaLookup
            .GroupBy(m => m.ModelId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.FileName).ToList());

        return products.Select(product => new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            PriceMin = product.PriceMin,
            PriceMax = product.PriceMax,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Description = product.Description,
            Status = product.Status,
            CompanyId = product.CompanyId,
            ImageUrls = mediaByProduct.TryGetValue(product.Id, out var urls) ? urls : new List<string>(),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }).ToList();
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
}
