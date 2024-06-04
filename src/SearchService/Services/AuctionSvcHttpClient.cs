using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<List<Item>> GetItemForSearchDb()
        {
            var lastUpdated = await DB.Find<Item, string>()
                .Sort(x => x.Descending(e => e.UpdatedAt))
                .Project(e => e.UpdatedAt.ToString())
                .ExecuteFirstAsync();
            System.Console.WriteLine("Last updated: " + lastUpdated);
            return await _httpClient.GetFromJsonAsync<List<Item>>(_configuration["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
        }
    }
}