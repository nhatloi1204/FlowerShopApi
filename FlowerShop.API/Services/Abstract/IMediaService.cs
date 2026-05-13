using CloudinaryDotNet.Actions;
using FlowerShop.API.Models.Entities;
using Microsoft.AspNetCore.Http;

public interface IMediaService
{
    Task<Media> SaveMediaAsync(ImageUploadResult uploadResult, IFormFile file, long modelId, string modelType, string collectionName = "gallery");
}