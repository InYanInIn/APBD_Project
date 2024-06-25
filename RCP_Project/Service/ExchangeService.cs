using System.Net;
using Newtonsoft.Json;
using RCP_Project.DTO.Exchange;

namespace RCP_Project.Service;

public class ExchangeService
{
    private readonly LocalDbContext _context;

    public ExchangeService(LocalDbContext context)
    {
        _context = context;
    }
    
    public async Task<decimal> GetExchangeRate(string currency)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("http://api.nbp.pl/api");
            var response = await client.GetAsync($"/exchangerates/rates/A/{currency}/?format=json");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // If the currency is not found, return 1 as the exchange rate
                return 1m;
            }

            response.EnsureSuccessStatusCode();

            var stringResult = await response.Content.ReadAsStringAsync();
            var rawRates = JsonConvert.DeserializeObject<NbpResponse>(stringResult);
            return rawRates.Rates[0].Mid;
        }
    }
}