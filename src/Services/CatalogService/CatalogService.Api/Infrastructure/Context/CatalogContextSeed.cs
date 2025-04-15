using CatalogService.Api.Core.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;
using Polly;
using System.Globalization;
using System.IO.Compression;

namespace CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {
            var policy = Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception, $"Exception with message detected on attempt of {retry} within time interval {timeSpan} for {ctx}");
                    }
                );

            var setupDirPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger));
        }

        private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath));

                await context.SaveChangesAsync();
            }

            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirPath));

                await context.SaveChangesAsync();
            }

            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));

                await context.SaveChangesAsync();

                GetCatalogItemPictures(setupDirPath, picturePath);
            }
        }

        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
            {
                return new List<CatalogBrand>()
            {
                new CatalogBrand() { Brand = "Azure" },
                new CatalogBrand() { Brand = ".NET" },
                new CatalogBrand() { Brand = "Visual Studio" },
                new CatalogBrand() { Brand = "SQL Server" },
                new CatalogBrand() { Brand = "Other" }
            };
            }

            string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");

            if (!File.Exists(fileName))
            {
                GetPreconfiguredCatalogBrands();
            }

            var fileContent = File.ReadAllLines(fileName);

            var brandList = fileContent.Select(b => new CatalogBrand()
            {
                Brand = b.Trim('"')
            }).Where(b => b != null);

            return brandList ?? GetPreconfiguredCatalogBrands();
        }

        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
            {
                return new List<CatalogType>()
            {
                new CatalogType() { Type = "Mug" },
                new CatalogType() { Type = "T-Shirt" },
                new CatalogType() { Type = "Sheet" },
                new CatalogType() { Type = "USB Memory Stick" },
            };
            }

            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");

            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogTypes();
            }

            var fileContent = File.ReadAllLines(fileName);

            var typeList = fileContent.Select(ct => new CatalogType()
            {
                Type = ct.Trim('"')
            }).Where(ct => ct != null);

            return typeList ?? GetPreconfiguredCatalogTypes();
        }

        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfiguredItems()
            {
                return new List<CatalogItem>()
                {
                new CatalogItem { CatalogTypeId = 2, CatalogBrandId = 2, AvailableStock = 100, Description = ".NET Bot Black Hoodie, and more", Name = ".NET Bot Black Hoodie", Price = 19.5m, PictureFileName = "1.png", OnReorder = false },
                new CatalogItem { CatalogTypeId = 1, CatalogBrandId = 5, AvailableStock = 89, Description = ".NET Black & White Mug", Name = ".NET Black & White Mug", Price = 8.5m, PictureFileName = "2.png", OnReorder = true },
                new CatalogItem { CatalogTypeId = 3, CatalogBrandId = 5, AvailableStock = 55, Description = "Roslyn Red Sheet", Name = "Roslyn Red Sheet", Price = 8.5m, PictureFileName = "5.png", OnReorder = false }
                };
            }

            string fileName = Path.Combine(contentPath, "CatalogItems.txt");

            if (!File.Exists(fileName))
                return GetPreconfiguredItems();

            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(ct => ct.Type, ct => ct.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(cb => cb.Brand, cb => cb.Id);

            List<CatalogItem> items = new List<CatalogItem>();

            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                parser.ReadLine(); // Skip header

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    try
                    {
                        items.Add(new CatalogItem
                        {
                            CatalogTypeId = catalogTypeIdLookup[fields[0]],
                            CatalogBrandId = catalogBrandIdLookup[fields[1]],
                            Description = fields[2],
                            Name = fields[3],
                            Price = decimal.Parse(fields[4], CultureInfo.InvariantCulture),
                            PictureFileName = fields[5],
                            AvailableStock = string.IsNullOrWhiteSpace(fields[6]) ? 0 : int.Parse(fields[6]),
                            OnReorder = bool.Parse(fields[7])
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred while parsing the following row: {string.Join(",", fields)}. Exception: {ex.Message}");
                    }
                }
            }

            return items;
        }

        private void GetCatalogItemPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";

            if (picturePath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(picturePath);

                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip");
                ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
            }
        }

    }
}
