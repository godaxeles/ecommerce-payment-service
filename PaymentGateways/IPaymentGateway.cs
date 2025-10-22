namespace PaymentService.PaymentGateways;

public interface IPaymentGateway
{
    Task<PaymentGatewayResponse> ProcessPaymentAsync(PaymentRequest request);
    Task<PaymentGatewayResponse> RefundPaymentAsync(string transactionId, decimal amount);
}

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = string.Empty;
}

public class PaymentGatewayResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}