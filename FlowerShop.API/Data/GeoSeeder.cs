using System.Text.Json;
using FlowerShop.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Data
{
    public class GeoSeeder
    {
        public static async Task SeedData(AppDbContext context)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "geo.json");

            Console.WriteLine($"Check file at: {Path.GetFullPath(filePath)}");
            if (!File.Exists(filePath)) return;

            using var jsonStream = File.OpenRead(filePath);
            using var document = await JsonDocument.ParseAsync(jsonStream);

            JsonElement root = document.RootElement;

            List<JsonElement> provinceElements = root.ValueKind == JsonValueKind.Array
                ? root.EnumerateArray().ToList()
                : new List<JsonElement> { root };

            foreach (var pElem in provinceElements)
            {
                if (!pElem.TryGetProperty("Code", out JsonElement codeElem)) continue;

                long pId = long.Parse(codeElem.GetString()!);
                string pName = pElem.GetProperty("FullName").GetString()!;

                var province = await context.Set<Province>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == pId);

                if (province == null)
                {
                    province = new Province { Id = pId, Name = pName, CreatedAt = DateTime.UtcNow };
                    context.Set<Province>().Add(province);
                }
                else
                {
                    province.Name = pName;
                    province.UpdatedAt = DateTime.UtcNow;
                    province.DeletedAt = null;
                }

                if (pElem.TryGetProperty("Wards", out JsonElement wardsElem) && wardsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var wElem in wardsElem.EnumerateArray())
                    {
                        long wId = long.Parse(wElem.GetProperty("Code").GetString()!);
                        string wName = wElem.GetProperty("FullName").GetString()!;

                        var ward = await context.Set<Ward>()
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(w => w.Id == wId);

                        if (ward == null)
                        {
                            context.Set<Ward>().Add(new Ward
                            {
                                Id = wId,
                                Name = wName,
                                ProvinceId = pId,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            ward.Name = wName;
                            ward.ProvinceId = pId;
                            ward.UpdatedAt = DateTime.UtcNow;
                            ward.DeletedAt = null;
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}