using System.Text.Json;
using CloudinaryDotNet.Actions;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;

namespace FlowerShop.API.Services.Concrete
{
    public class MediaService : IMediaService
    {
        private readonly AppDbContext _context;

        public MediaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Media> SaveMediaAsync(
            ImageUploadResult uploadResult,
            IFormFile file,
            long modelId,
            string modelType,
            string collectionName = "gallery")
        {
            var customProps = new
            {
                public_id = uploadResult.PublicId,
                version = uploadResult.Version,
                format = uploadResult.Format
            };

            var media = new Media
            {
                ModelType = modelType,
                ModelId = modelId,
                Uuid = Guid.NewGuid(),
                CollectionName = collectionName,

                Name = Path.GetFileNameWithoutExtension(file.FileName),
                FileName = uploadResult.SecureUrl.ToString(),

                MimeType = file.ContentType,
                Disk = "cloudinary",
                Size = uploadResult.Bytes,

                CustomProperties = JsonSerializer.Serialize(customProps),
                Manipulations = "{}",
                GeneratedConversions = "{}",
                ResponsiveImages = "{}",

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Medias.Add(media);
            await _context.SaveChangesAsync();

            return media;
        }
    }
}
