namespace PaymentService.Models.DTOs;

public class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
}