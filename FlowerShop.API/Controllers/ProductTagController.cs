using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
public class ProductTagController : ControllerBase
{
    private readonly IProductTagService _tagService;

    public ProductTagController(IProductTagService tagService)
    {
        _tagService = tagService;
    }

    // ----------- PUBLIC ROUTES -----------

    [HttpGet("/api/product-tags")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _tagService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("/api/product-tags/{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _tagService.GetByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    // ----------- ADMIN ROUTES -----------

    [HttpPost("api/admin/product-tags")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] ProductTagInputResource request)
    {
        var result = await _tagService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    [HttpPut("api/admin/product-tags/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(long id, [FromBody] ProductTagInputResource request)
    {
        var result = await _tagService.UpdateAsync(id, request);
        if (!result.Success)
        {
            if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("api/admin/product-tags/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _tagService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}