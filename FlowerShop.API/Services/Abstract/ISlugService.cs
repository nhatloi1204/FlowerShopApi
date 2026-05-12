namespace FlowerShop.API.Services.Abstract;

public interface ISlugService
{
    Task<string> GenerateUniqueSlugAsync<T>(string name, IQueryable<T> dbSet) where T : class;
}
