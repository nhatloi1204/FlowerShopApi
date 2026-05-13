namespace FlowerShop.API.Services.Abstract;

using CloudinaryDotNet.Actions;

public interface ICloudinaryService
{
    Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string folderName);
    Task<bool> DeleteImageAsync(string publicId);
}
