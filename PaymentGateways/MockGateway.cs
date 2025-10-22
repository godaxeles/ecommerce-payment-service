namespace PaymentService.PaymentGateways;

public class MockGateway : IPaymentGateway
{
    private readonly ILogger<MockGateway> _logger;

    public MockGateway(ILogger<MockGateway> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentGatewayResponse> ProcessPaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("Processing mock payment for {Amount} {Currency}",
            request.Amount, request.Currency);

        // Simulate payment processing delay
        await Task.Delay(1000);

        // Mock successful payment (90% success rate)
        var success = Random.Shared.Next(100) < 90;

        return new PaymentGatewayResponse
        {
            Success = success,
            TransactionId = success ? Guid.NewGuid().ToString() : null,
            ErrorMessage = success ? null : "Mock payment failed"
        };
    }

    public async Task<PaymentGatewayResponse> RefundPaymentAsync(string transactionId, decimal amount)
    {
        _logger.LogInformation("Processing mock refund for transaction {TransactionId}",
            transactionId);

        await Task.Delay(500);

        return new PaymentGatewayResponse
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString()
        };
    }
}