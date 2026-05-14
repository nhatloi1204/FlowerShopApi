using CloudinaryDotNet.Actions;
using FlowerShop.API.Models.Views;
using Microsoft.AspNetCore.Http;

public interface IMediaService
{
    Task<MediaOutputResource> SaveMediaAsync(ImageUploadResult uploadResult, IFormFile file, long modelId, string modelType, string collectionName = "gallery");
}