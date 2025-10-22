using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Models.DTOs;
using PaymentService.PaymentGateways;

namespace PaymentService.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        PaymentDbContext context,
        IPaymentGateway paymentGateway,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentRequest request)
    {
        // Create payment record
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            Method = request.Method,
            Status = PaymentStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        try
        {
            // Process through payment gateway
            var result = await _paymentGateway.ProcessPaymentAsync(new PaymentRequest
            {
                Amount = request.Amount,
                Currency = "USD",
                PaymentMethod = request.Method.ToString()
            });

            payment.ExternalPaymentId = result.TransactionId;
            payment.Status = result.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment processing failed");
            payment.Status = PaymentStatus.Failed;
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<PaymentDto?> ConfirmPaymentAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return null;

        payment.Status = PaymentStatus.Completed;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> RefundPaymentAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null || payment.Status != PaymentStatus.Completed)
            return null;

        payment.Status = PaymentStatus.Refunded;
        payment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    public async Task<List<PaymentDto>> GetUserPaymentsAsync(Guid userId)
    {
        var payments = await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    private static PaymentDto MapToDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Status = payment.Status.ToString(),
            Method = payment.Method.ToString(),
            CreatedAt = payment.CreatedAt
        };
    }
}