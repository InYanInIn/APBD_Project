using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RCP_Project.DTO;
using RCP_Project.DTO.Exchange;
using RCP_Project.Service;

namespace RCP_Project.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly ExchangeService _exchangeService;

        public RevenueController(LocalDbContext context, ExchangeService exchangeService)
        {
            _context = context;
            _exchangeService = exchangeService;
        }

        // GET: api/Revenue/Current
        [Authorize]
        [HttpGet("Current")]
        public async Task<ActionResult<decimal>> CalculateCurrentRevenue(int? productID, string currency)
        {
            var payments = _context.Payments.AsQueryable();

            if (productID.HasValue)
            {
                payments = payments.Where(p => p.Contract.SoftwareID == productID.Value);
            }

            var revenue = await payments.SumAsync(p => p.Amount);

            if (!string.IsNullOrEmpty(currency))
            {
                var exchangeRate = await _exchangeService.GetExchangeRate(currency);
                revenue *= exchangeRate;
            }

            return revenue;
        }

        // GET: api/Revenue/Predicted
        [Authorize]
        [HttpGet("Predicted")]
        public async Task<ActionResult<decimal>> CalculatePredictedRevenue(int? productID, string currency)
        {
            var contracts = _context.Contracts.AsQueryable();

            if (productID.HasValue)
            {
                contracts = contracts.Where(c => c.SoftwareID == productID.Value);
            }

            var revenue = await contracts.SumAsync(c => c.Price);

            if (!string.IsNullOrEmpty(currency))
            {
                var exchangeRate = await _exchangeService.GetExchangeRate(currency);
                revenue *= exchangeRate;
            }

            return revenue;
        }

        

    }
}