namespace PaymentService.Controllers;

using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.DTOs;
using PaymentService.Services;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var payment = await _paymentService.ProcessPaymentAsync(request);
        return Ok(payment);
    }

    [HttpPost("{id}/confirm")]
    public async Task<ActionResult<PaymentDto>> Confirm(Guid id)
    {
        var payment = await _paymentService.ConfirmPaymentAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpPost("{id}/refund")]
    public async Task<ActionResult<PaymentDto>> Refund(Guid id)
    {
        var payment = await _paymentService.RefundPaymentAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<PaymentDto>>> GetHistory()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var payments = await _paymentService.GetUserPaymentsAsync(Guid.Parse(userId));
        return Ok(payments);
    }
}