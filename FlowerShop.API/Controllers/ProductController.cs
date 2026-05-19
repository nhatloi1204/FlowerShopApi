using FlowerShop.API.Models.Views;
using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// using FlowerShop.API.Constants; 

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // ----------- PUBLIC ROUTE -----------

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryResource query)
    {
        var result = await _productService.GetProductsAsync(query);
        return Ok(result);
    }

    [HttpGet("products/{identifier}")]
    public async Task<IActionResult> GetByIdOrSlug(string identifier)
    {
        if (long.TryParse(identifier, out long id))
        {
            var idResult = await _productService.GetByIdAsync(id);
            return idResult.Success ? Ok(idResult) : NotFound(idResult);
        }

        var slugResult = await _productService.GetBySlugAsync(identifier);
        return slugResult.Success ? Ok(slugResult) : NotFound(slugResult);
    }

    // ----------- ADMIN ROUTE -----------

    [HttpPost("/api/admin/products")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] ProductInputResource request)
    {
        var result = await _productService.CreateAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetByIdOrSlug), new { identifier = result.Data?.Id }, result);
    }

    [HttpPut("/api/admin/products/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(long id, [FromForm] ProductInputResource request)
    {
        var result = await _productService.UpdateAsync(id, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("/api/admin/products/{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}