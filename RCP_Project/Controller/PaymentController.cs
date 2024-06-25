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
    public class PaymentController : ControllerBase
    {
        private readonly LocalDbContext _context;
        private readonly PaymentService _paymentService;

        public PaymentController(LocalDbContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // GET: api/Payment/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Payment>> GetPayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }
        
        // POST: api/Payment/PayForContract
        [Authorize]
        [HttpPost("PayForContract")]
        public async Task<ActionResult<Payment>> PayForContract(PaymentCreateDTO paymentDTO)
        {
            // Validate the input data
            if (paymentDTO.Amount <= 0)
            {
                return BadRequest("The payment amount should be positive.");
            }

            // Check if the contract exists and is not already fully paid
            var contract = await _context.Contracts.FindAsync(paymentDTO.ContractID);
            if (contract == null)
            {
                return NotFound("The contract does not exist.");
            }

            if (contract.IsPaid)
            {
                return Conflict("The contract is already fully paid.");
            }

            if (paymentDTO.PaymentDate > contract.EndDate)
            {
                await _paymentService.RefundPayments(paymentDTO.ContractID);
                return BadRequest("Cannot accept the payment for a contract after the date specified within the" +
                                  " contract. Refund process started.");
            }

            var payment = new Payment
            {
                ContractID = paymentDTO.ContractID,
                Amount = paymentDTO.Amount,
                PaymentDate = paymentDTO.PaymentDate
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Check if the contract is fully paid
            var totalPayments = await _context.Payments
                .Where(p => p.ContractID == paymentDTO.ContractID)
                .SumAsync(p => p.Amount);
            if (totalPayments >= contract.Price)
            {
                contract.IsPaid = true;
                _context.Entry(contract).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            
            contract.IsActive = true;
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return CreatedAtAction("GetPayment", new { id = payment.ID }, payment);
        }       
        
        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.ID == id);
        }
    }
}
