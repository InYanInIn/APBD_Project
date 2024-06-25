using Microsoft.EntityFrameworkCore;
using RCP_Project.Models;

namespace RCP_Project.Service;

public class PaymentService
{
    private readonly LocalDbContext _context;

    public PaymentService(LocalDbContext context)
    {
        _context = context;
    }

    public async Task RefundPayments(int contractID)
    {
        var contract = await _context.Contracts.FindAsync(contractID);
        if (contract == null)
        {
            throw new Exception("The contract does not exist.");
        }

        var payments = await _context.Payments
            .Where(p => p.ContractID == contractID)
            .ToListAsync();

        foreach (var payment in payments)
        {
            var refund = new Payment
            {
                ContractID = contractID,
                Amount = -payment.Amount,
                PaymentDate = DateTime.Now
            };

            _context.Payments.Add(refund);
        }

        contract.IsPaid = false;
        _context.Entry(contract).State = EntityState.Modified;

        await _context.SaveChangesAsync();
    }
}