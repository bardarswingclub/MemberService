namespace MemberService.Services.Vipps;
using System.Threading.Tasks;

using MemberService.Configs;
using MemberService.Services.Vipps.Models;

public class VippsClient : IVippsClient
{
    private readonly HttpClient _httpClient;
    private readonly Config _config;

    public VippsClient(
        Config config,
        IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Vipps");
        _config = config;
    }

    public async Task<InitiatePaymentResponse> InitiatePayment(Transaction transaction, string secret, string returnUrl)
    {
        var response = await _httpClient.PostAsJsonAsync("/ecomm/v2/payments", new InitiatePaymentRequest
        {
            MerchantInfo = new()
            {
                AuthToken = secret,
                MerchantSerialNumber = _config.Vipps.MerchantSerialNumber,
                CallbackPrefix = _config.Vipps.CallbackPrefix,
                FallBack = returnUrl,
            },
            CustomerInfo = new(),
            Transaction = transaction
        });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InitiatePaymentResponse>();
    }

    public async Task<CapturePaymentResponse> CapturePayment(string orderId, string transactionText)
    {
        var response = await _httpClient.PostAsJsonAsync($"/ecomm/v2/payments/{orderId}/capture", new CapturePaymentRequest
        {
            MerchantInfo = new(_config.Vipps.MerchantSerialNumber),
            Transaction = new(0, transactionText)
        });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CapturePaymentResponse>();
    }

    public async Task<PaymentDetails> GetPaymentDetails(string orderId)
    {
        return await _httpClient.GetFromJsonAsync<PaymentDetails>($"/ecomm/v2/payments/{orderId}/details");
    }
}
