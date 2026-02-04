using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QuanLyRuiRoTinDung.Services
{
    public interface IZaloPayService
    {
        Task<ZaloPayOrderResult> CreateOrderAsync(ZaloPayOrderRequest request);
        Task<ZaloPayQueryResult> QueryOrderAsync(string appTransId);
        bool ValidateCallback(string data, string mac);
        string GenerateAppTransId();
    }

    public class ZaloPayService : IZaloPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZaloPayService> _logger;
        private readonly HttpClient _httpClient;

        private string AppId => _configuration["ZaloPay:AppId"] ?? "554";
        private string Key1 => _configuration["ZaloPay:Key1"] ?? "";
        private string Key2 => _configuration["ZaloPay:Key2"] ?? "";
        private string AppUser => _configuration["ZaloPay:AppUser"] ?? "BankCRM";
        private string CreateOrderEndpoint => _configuration["ZaloPay:CreateOrderEndpoint"] ?? "https://sb-openapi.zalopay.vn/v2/create";
        private string QueryOrderEndpoint => _configuration["ZaloPay:QueryOrderEndpoint"] ?? "https://sb-openapi.zalopay.vn/v2/query";
        private string CallbackUrl => _configuration["ZaloPay:CallbackUrl"] ?? "";
        private string RedirectUrl => _configuration["ZaloPay:RedirectUrl"] ?? "";

        public ZaloPayService(IConfiguration configuration, ILogger<ZaloPayService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ZaloPay");
        }

        public string GenerateAppTransId()
        {
            var now = DateTime.Now;
            var random = new Random();
            return $"{now:yyMMdd}_{now:HHmmss}{random.Next(1000, 9999)}";
        }

        public async Task<ZaloPayOrderResult> CreateOrderAsync(ZaloPayOrderRequest request)
        {
            try
            {
                var appTransId = GenerateAppTransId();
                var appTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var embedData = JsonSerializer.Serialize(new
                {
                    redirecturl = RedirectUrl,
                    maGiaoDich = request.MaGiaoDich,
                    maKhoanVay = request.MaKhoanVay,
                    kyTraNo = request.KyTraNo
                });

                var items = JsonSerializer.Serialize(new[]
                {
                    new
                    {
                        itemid = request.MaGiaoDich.ToString(),
                        itemname = $"Thanh toán kỳ {request.KyTraNo} - Khoản vay {request.MaKhoanVayCode}",
                        itemprice = (long)request.SoTien,
                        itemquantity = 1
                    }
                });

                // Tạo chuỗi ký HMAC
                // data = appid|app_trans_id|appuser|amount|apptime|embeddata|item
                var dataToSign = $"{AppId}|{appTransId}|{AppUser}|{(long)request.SoTien}|{appTime}|{embedData}|{items}";
                var mac = ComputeHmacSha256(dataToSign, Key1);

                var orderData = new Dictionary<string, string>
                {
                    { "app_id", AppId },
                    { "app_trans_id", appTransId },
                    { "app_user", AppUser },
                    { "app_time", appTime.ToString() },
                    { "amount", ((long)request.SoTien).ToString() },
                    { "description", $"Bank CRM - Thanh toán kỳ {request.KyTraNo} khoản vay {request.MaKhoanVayCode}" },
                    { "embed_data", embedData },
                    { "item", items },
                    { "bank_code", "" },
                    { "mac", mac }
                };

                // Thêm callback_url nếu có
                if (!string.IsNullOrEmpty(CallbackUrl))
                {
                    orderData["callback_url"] = CallbackUrl;
                }

                var content = new FormUrlEncodedContent(orderData);
                var response = await _httpClient.PostAsync(CreateOrderEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("ZaloPay CreateOrder Response: {Response}", responseContent);

                var result = JsonSerializer.Deserialize<ZaloPayCreateOrderResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null && result.Return_code == 1)
                {
                    return new ZaloPayOrderResult
                    {
                        Success = true,
                        AppTransId = appTransId,
                        OrderUrl = result.Order_url,
                        ZpTransToken = result.Zp_trans_token,
                        QrCode = result.Order_url, // URL có thể dùng để tạo QR
                        Message = "Tạo đơn thanh toán thành công"
                    };
                }
                else
                {
                    return new ZaloPayOrderResult
                    {
                        Success = false,
                        Message = result?.Return_message ?? "Không thể tạo đơn thanh toán"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZaloPay order");
                return new ZaloPayOrderResult
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ZaloPayQueryResult> QueryOrderAsync(string appTransId)
        {
            try
            {
                var dataToSign = $"{AppId}|{appTransId}|{Key1}";
                var mac = ComputeHmacSha256(dataToSign, Key1);

                var queryData = new Dictionary<string, string>
                {
                    { "app_id", AppId },
                    { "app_trans_id", appTransId },
                    { "mac", mac }
                };

                var content = new FormUrlEncodedContent(queryData);
                var response = await _httpClient.PostAsync(QueryOrderEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("ZaloPay QueryOrder Response: {Response}", responseContent);

                var result = JsonSerializer.Deserialize<ZaloPayQueryResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    return new ZaloPayQueryResult
                    {
                        ReturnCode = result.Return_code,
                        ReturnMessage = result.Return_message,
                        IsProcessing = result.Is_processing,
                        Amount = result.Amount,
                        ZpTransId = result.Zp_trans_id
                    };
                }

                return new ZaloPayQueryResult
                {
                    ReturnCode = -1,
                    ReturnMessage = "Không thể truy vấn đơn hàng"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying ZaloPay order");
                return new ZaloPayQueryResult
                {
                    ReturnCode = -1,
                    ReturnMessage = $"Lỗi: {ex.Message}"
                };
            }
        }

        public bool ValidateCallback(string data, string mac)
        {
            try
            {
                var computedMac = ComputeHmacSha256(data, Key2);
                return computedMac.Equals(mac, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ZaloPay callback");
                return false;
            }
        }

        private string ComputeHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // Request/Response classes
    public class ZaloPayOrderRequest
    {
        public int MaGiaoDich { get; set; }
        public int MaKhoanVay { get; set; }
        public string MaKhoanVayCode { get; set; } = "";
        public int KyTraNo { get; set; }
        public decimal SoTien { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
    }

    public class ZaloPayOrderResult
    {
        public bool Success { get; set; }
        public string? AppTransId { get; set; }
        public string? OrderUrl { get; set; }
        public string? ZpTransToken { get; set; }
        public string? QrCode { get; set; }
        public string? Message { get; set; }
    }

    public class ZaloPayQueryResult
    {
        public int ReturnCode { get; set; }
        public string? ReturnMessage { get; set; }
        public bool IsProcessing { get; set; }
        public long Amount { get; set; }
        public long ZpTransId { get; set; }
    }

    public class ZaloPayCreateOrderResponse
    {
        public int Return_code { get; set; }
        public string? Return_message { get; set; }
        public int Sub_return_code { get; set; }
        public string? Sub_return_message { get; set; }
        public string? Order_url { get; set; }
        public string? Zp_trans_token { get; set; }
        public string? Order_token { get; set; }
        public string? Qr_code { get; set; }
    }

    public class ZaloPayQueryResponse
    {
        public int Return_code { get; set; }
        public string? Return_message { get; set; }
        public int Sub_return_code { get; set; }
        public string? Sub_return_message { get; set; }
        public bool Is_processing { get; set; }
        public long Amount { get; set; }
        public long Zp_trans_id { get; set; }
        public string? Server_time { get; set; }
    }

    public class ZaloPayCallbackData
    {
        public int App_id { get; set; }
        public string? App_trans_id { get; set; }
        public long App_time { get; set; }
        public string? App_user { get; set; }
        public long Amount { get; set; }
        public string? Embed_data { get; set; }
        public string? Item { get; set; }
        public long Zp_trans_id { get; set; }
        public long Server_time { get; set; }
        public int Channel { get; set; }
        public string? Merchant_user_id { get; set; }
        public long User_fee_amount { get; set; }
        public long Discount_amount { get; set; }
    }
}
