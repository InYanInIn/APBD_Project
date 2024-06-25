using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RCP_Project.DTO;
using RCP_Project.Models;
using RCP_Project.Service;

namespace RCP_Project.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly PaymentService _paymentService;

        public ContractController(LocalDbContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }
        
        // GET: api/Contract/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            return contract;
        }

        // POST: api/Contract/CreateContract
        [Authorize]
        [HttpPost("CreateContract")]
        public async Task<ActionResult<Contract>> CreateContract(ContractCreateDTO contractDTO)
        {
            // Validate the input data
            var timeRange = (contractDTO.EndDate - contractDTO.StartDate).TotalDays;
            if (timeRange < 3 || timeRange > 30)
            {
                return BadRequest("The time range should be at least 3 days and at most 30 days.");
            }

            if (contractDTO.SupportExtensionYears < 1 || contractDTO.SupportExtensionYears > 3)
            {
                return BadRequest("The support can only be extended by 1, 2 or 3 years.");
            }

            // Calculate the price
            var software = await _context.Software.FindAsync(contractDTO.SoftwareID);
            var basePrice = software.UpfrontCost;
            var discount = await _context.Discounts
                .Where(d => d.AppliesTo == "Contract")
                .OrderByDescending(d => d.Value)
                .FirstOrDefaultAsync();
            var discountValue = discount != null ? discount.Value : 0;
            var isReturningClient = await _context.Contracts.AnyAsync(c => c.ClientID == contractDTO.ClientID);
            if (isReturningClient)
            {
                discountValue += 0.05;
            }

            var price = basePrice * (1 - (decimal)discountValue) + (contractDTO.SupportExtensionYears - 1) * 1000;

            // Check if the client already has an active contract for the software
            var hasActiveContract = await _context.Contracts.AnyAsync(c =>
                c.ClientID == contractDTO.ClientID &&
                c.SoftwareID == contractDTO.SoftwareID &&
                c.IsActive);
            if (hasActiveContract)
            {
                return Conflict("The client already has an active contract for this software.");
            }

            // Create a new Contract
            var contract = new Contract
            {
                ClientID = contractDTO.ClientID,
                SoftwareID = contractDTO.SoftwareID,
                StartDate = contractDTO.StartDate,
                EndDate = contractDTO.EndDate,
                Price = price,
                Version = software.CurrentVersion,
                IsActive = false, 
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContract", new { id = contract.ID }, contract);
        }
        
        
        
        // GET: api/Contract
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetContracts()
        {
            return await _context.Contracts.ToListAsync();
        }

        // DELETE: api/Contract/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.ID == id);
        }
    }
}
