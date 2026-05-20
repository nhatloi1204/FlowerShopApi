using FlowerShop.API.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public MediaController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("upload-raw")]
    public async Task<IActionResult> UploadRaw(IFormFile file)
    {
        var result = await _cloudinaryService.UploadImageAsync(file, "products");
        if (result == null) return BadRequest("Failed to upload image");

        return Ok(new
        {
            url = result.SecureUrl.AbsoluteUri,
            publicId = result.PublicId
        });
    }

    [HttpDelete("delete-raw")]
    public async Task<IActionResult> DeleteRaw([FromQuery] string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return BadRequest("PublicId is invalid");

        var success = await _cloudinaryService.DeleteImageAsync(publicId);
        if (!success) return BadRequest("Failed to delete image on Cloudinary");

        return Ok(new { Message = "Image deleted successfully" });
    }

    [HttpPost("delete-multi")]
    public async Task<IActionResult> DeleteMulti([FromBody] List<string> publicIds)
    {
        if (publicIds == null || publicIds.Count == 0) return Ok();

        var deleteTasks = publicIds.Select(id => _cloudinaryService.DeleteImageAsync(id));
        await Task.WhenAll(deleteTasks);

        return Ok(new { Message = "Cleaned up unused images successfully" });
    }
}