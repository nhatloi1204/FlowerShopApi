using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FlowerShop.API.Services.Abstract;

namespace FlowerShop.API.Services.Concrete
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["CloudinarySettings:CloudName"],
                config["CloudinarySettings:ApiKey"],
                config["CloudinarySettings:ApiSecret"]
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult?> UploadImageAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length <= 0) return null;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"FlowerShop/{folderName}",

                // Optimize the image for web delivery
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null) throw new Exception(result.Error.Message);

            return result;
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            return result.Result == "ok";
        }
    }
}
