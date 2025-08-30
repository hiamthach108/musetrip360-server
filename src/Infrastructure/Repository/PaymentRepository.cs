using Database;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;

public interface IPaymentRepository
{
    Task<Payment> GetByIdAsync(Guid id);
    Task AddAsync(Payment payment);
    Task UpdateAsync(Guid id, Payment payment);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<Payment>> GetPaymentByUserIdAsync(Guid userId);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly MuseTrip360DbContext _dbContext;
    public PaymentRepository(MuseTrip360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment)
    {
        await _dbContext.Payments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var payment = await _dbContext.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new Exception("Payment not found");
        }
        _dbContext.Payments.Remove(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Payment> GetByIdAsync(Guid id)
    {
        var payment = await _dbContext.Payments.FindAsync(id);
        if (payment == null)
        {
            throw new Exception("Payment not found");
        }
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetPaymentByUserIdAsync(Guid userId)
    {
        var payments = await _dbContext.Payments
        .Where(p => p.CreatedBy == userId)
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
        return payments;
    }

    public async Task UpdateAsync(Guid id, Payment payment)
    {
        var existingPayment = await _dbContext.Payments.FindAsync(id);
        if (existingPayment == null)
        {
            throw new Exception("Payment not found");
        }
        _dbContext.Entry(existingPayment).CurrentValues.SetValues(payment);
        await _dbContext.SaveChangesAsync();
    }
}