using Microsoft.AspNetCore.Mvc;
using PSP.Application.DTOs.Payments;
using PSP.Infrastructure.Services;

namespace PSP.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("initialize")]
    public async Task<ActionResult<PaymentInitializationResponse>> InitializePayment([FromBody] PaymentInitializationRequest request)
    {
        try
        {
            var response = await _paymentService.InitializePaymentAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetPayment(int id, [FromQuery] string? token)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound(new { message = "Payment not found" });
            }

            // Validate access token
            if (string.IsNullOrEmpty(token) || payment.AccessToken != token)
            {
                return Unauthorized(new { message = "Invalid or missing access token" });
            }

            var availablePaymentMethods = payment.WebShop.WebShopPaymentMethods
                .Select(wpm => new
                {
                    id = wpm.PaymentMethodId,
                    name = wpm.PaymentMethod.Name,
                    type = wpm.PaymentMethod.Type.ToString()
                })
                .ToList();

            return Ok(new
            {
                id = payment.Id,
                webShopId = payment.WebShopId,
                webShopName = payment.WebShop.Name,
                amount = payment.Amount,
                currency = payment.Currency.ToString(),
                status = payment.Status.ToString(),
                merchantOrderId = payment.MerchantOrderId,
                availablePaymentMethods = availablePaymentMethods
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving payment", error = ex.Message });
        }
    }

    [HttpPost("{id}/select-method")]
    public async Task<ActionResult> SelectPaymentMethod(int id, [FromBody] SelectPaymentMethodRequest request)
    {
        try
        {
            var bankRequestData = await _paymentService.SelectPaymentMethodAsync(id, request.PaymentMethodId);
            return Ok(bankRequestData);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult> CancelPayment(int id)
    {
        try
        {
            await _paymentService.CancelPaymentAsync(id);
            return Ok(new { message = "Payment cancelled" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    [HttpGet("{id}/bank-callback")]
    public async Task<ActionResult> HandleBankCallback(int id, [FromQuery] string status, [FromQuery] string? paymentId)
    {
        try
        {
            var redirectUrl = await _paymentService.HandleBankCallbackAsync(id, status, paymentId);
            return Redirect(redirectUrl);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}

public record SelectPaymentMethodRequest(int PaymentMethodId);
