using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FlowerShop.API.Data;
using FlowerShop.API.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Services.Concrete;

public class SlugService : ISlugService
{
    public async Task<string> GenerateUniqueSlugAsync<T>(string name, IQueryable<T> dbSet) where T : class
    {
        var baseSlug = Slugify(name);

        var existingSlugs = await dbSet
                            .Select(e => EF.Property<string>(e, "Slug"))
                            .Where(slug => slug == baseSlug || slug.StartsWith(baseSlug + "-"))
                            .ToListAsync();

        if (!existingSlugs.Contains(baseSlug))
        {
            return baseSlug;
        }

        var counter = 1;
        var newSlug = $"{baseSlug}-{counter}";

        while (existingSlugs.Contains(newSlug))
        {
            counter++;
            newSlug = $"{baseSlug}-{counter}";
        }

        return newSlug;
    }

    private static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        var cleaned = builder.ToString().Normalize(NormalizationForm.FormC);
        cleaned = Regex.Replace(cleaned, @"[^a-z0-9]+", "-");
        cleaned = Regex.Replace(cleaned, @"-+", "-").Trim('-');

        return cleaned;
    }
}
