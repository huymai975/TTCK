using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WebAppBookingBoat.Services
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayComparer());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayComparer());

        public void AddRequestData(string key, string value) => _requestData.Add(key, value);
        public void AddResponseData(string key, string value) => _responseData.Add(key, value);
        public string GetResponseData(string key) => _responseData.TryGetValue(key, out var val) ? val : string.Empty;

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    // Sử dụng WebUtility.UrlEncode và ép kiểu viết hoa
                    string encodedKey = WebUtility.UrlEncode(kv.Key);
                    string encodedValue = WebUtility.UrlEncode(kv.Value);
                    data.Append(encodedKey + "=" + encodedValue + "&");
                }
            }

            string queryString = data.ToString().TrimEnd('&');

            // ĐỒNG BỘ TUYỆT ĐỐI: 
            // .NET encode mặc định là chữ thường (%2f), VNPay cần chữ hoa (%2F)
            queryString = queryString.Replace("%20", "+")
                                     .Replace("%2f", "%2F")
                                     .Replace("%3a", "%3A")
                                     .Replace("%2b", "%2B");

            string vnp_SecureHash = HmacSHA512(vnp_HashSecret, queryString);
            return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _responseData)
            {
                if (!String.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    string encodedKey = WebUtility.UrlEncode(kv.Key);
                    string encodedValue = WebUtility.UrlEncode(kv.Value);
                    data.Append(encodedKey + "=" + encodedValue + "&");
                }
            }

            string signData = data.ToString().TrimEnd('&');

            // BẮT BUỘC: Ép các ký tự encode thành chữ HOA (ví dụ %3a -> %3A)
            // VNPay không chấp nhận chữ thường trong chuỗi băm.
            signData = signData.Replace("%2b", "%2B")
                               .Replace("%2f", "%2F")
                               .Replace("%3a", "%3A")
                               .Replace("%20", "+"); // Dấu cách chuyển thành + theo chuẩn VNPay

            string checkSum = HmacSHA512(secretKey, signData);
            return checkSum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                // VNPay yêu cầu chuỗi Hash phải viết HOA toàn bộ
                return BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
            }
        }
    }

    public class VnPayComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}