using FlowerShop.API.Models.DTOs.ProductCategory;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryService _categoryService;

    public ProductCategoryController(IProductCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // ----------- PUBLIC ROUTE -----------

    [HttpGet("/api/product-categories")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("/api/product-categories/{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    // ----------- ADMIN ROUTE -----------

    [HttpPost("api/admin/product-categories")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("api/admin/product-categories/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("api/admin/product-categories/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
