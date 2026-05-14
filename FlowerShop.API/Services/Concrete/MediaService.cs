using System.Text.Json;
using AutoMapper;
using CloudinaryDotNet.Actions;
using FlowerShop.API.Data;
using FlowerShop.API.Models.Entities;
using FlowerShop.API.Models.Views;

namespace FlowerShop.API.Services.Concrete
{
    public class MediaService : IMediaService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MediaService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MediaOutputResource> SaveMediaAsync(
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

            return _mapper.Map<MediaOutputResource>(media);
        }
    }
}
