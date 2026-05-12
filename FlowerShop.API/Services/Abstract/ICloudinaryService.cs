namespace FlowerShop.API.Services.Abstract;
using FlowerShop.API.Models.DTOs.Cloudinary;

public interface ICloudinaryService
{
    Task<CloudinaryResponse?> UploadImageAsync(IFormFile file, string folderName);
    Task<bool> DeleteImageAsync(string publicId);
}
