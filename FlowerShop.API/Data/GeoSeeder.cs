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

            if (!File.Exists(filePath)) return;

            if (await context.Set<Province>().AnyAsync())
            {
                Console.WriteLine("Geo data already seeded. Skipping...");
                return;
            }

            using var jsonStream = File.OpenRead(filePath);
            using var document = await JsonDocument.ParseAsync(jsonStream);

            JsonElement root = document.RootElement;

            List<JsonElement> provinceElements = root.ValueKind == JsonValueKind.Array
                ? root.EnumerateArray().ToList()
                : new List<JsonElement> { root };

            var existingProvinces = await context.Set<Province>().IgnoreQueryFilters().ToDictionaryAsync(p => p.Id);
            var existingWards = await context.Set<Ward>().IgnoreQueryFilters().ToDictionaryAsync(w => w.Id);

            var now = DateTime.UtcNow;

            foreach (var pElem in provinceElements)
            {
                if (!pElem.TryGetProperty("Code", out JsonElement codeElem)) continue;

                long pId = long.Parse(codeElem.GetString()!);
                string pName = pElem.GetProperty("FullName").GetString()!;

                var province = await context.Set<Province>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Id == pId);

                if (!existingProvinces.TryGetValue(pId, out province))
                {
                    province = new Province { Id = pId, Name = pName, CreatedAt = now };
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

                        if (!existingWards.TryGetValue(wId, out ward))
                        {
                            context.Set<Ward>().Add(new Ward
                            {
                                Id = wId,
                                Name = wName,
                                ProvinceId = pId,
                                CreatedAt = now
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

            }
            await context.SaveChangesAsync();
        }
    }
}