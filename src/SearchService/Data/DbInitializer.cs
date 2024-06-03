using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            await DB.InitAsync("SearchDb", MongoClientSettings
                .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            await DB.Index<Item>()
                .Key(a => a.Make, KeyType.Text)
                .Key(a => a.Model, KeyType.Text)
                .Key(a => a.Color, KeyType.Text)
                .CreateAsync();

            var count = await DB.CountAsync<Item>();

            if (count == 0)
            {
                System.Console.WriteLine("No Data - will attempt to seed");
                var itemData = await File.ReadAllTextAsync("Data/auction.json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                var item = JsonSerializer.Deserialize<List<Item>>(itemData, options);

                await DB.SaveAsync(item);
            }
        }
    }
}