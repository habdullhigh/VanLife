namespace VanLife.Api.Services;

public interface IPaymentService
{
    Task<bool> ChargeAsync(string paymentToken, decimal amount);
}

public class PaymentService : IPaymentService
{
    // In production replace with real gateway integration
    public Task<bool> ChargeAsync(string paymentToken, decimal amount)
    {
        // accept any non-empty token and amount > 0 for demo
        if (string.IsNullOrWhiteSpace(paymentToken) || amount <= 0) return Task.FromResult(false);
        return Task.FromResult(true);
    }
}
