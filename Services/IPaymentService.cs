using PaymentService.Models.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentDto> ProcessPaymentAsync(CreatePaymentRequest request);
    Task<PaymentDto?> ConfirmPaymentAsync(Guid id);
    Task<PaymentDto?> RefundPaymentAsync(Guid id);
    Task<List<PaymentDto>> GetUserPaymentsAsync(Guid userId);
}