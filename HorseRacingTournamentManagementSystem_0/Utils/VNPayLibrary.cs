using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace HorseRacingTournamentManagementSystem_0.Utils;

public class VNPayLibrary
{
    private SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
    private SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        var data = new StringBuilder();
        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(UrlEncode(kv.Key) + "=" + UrlEncode(kv.Value) + "&");
            }
        }
        string queryString = data.ToString();
        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1);
        }

        string signData = queryString;
        string vnp_SecureHash = VNPayHelper.HmacSHA512(vnp_HashSecret, signData);

        baseUrl += "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseData();
        string myChecksum = VNPayHelper.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, System.StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }
        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }
        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(UrlEncode(kv.Key) + "=" + UrlEncode(kv.Value) + "&");
            }
        }
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }
        return data.ToString();
    }

    private string UrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var encoded = WebUtility.UrlEncode(value);
        
        // Force uppercase hex for VNPAY compatibility
        var result = new StringBuilder();
        for (int i = 0; i < encoded.Length; i++)
        {
            if (encoded[i] == '%' && i + 2 < encoded.Length)
            {
                result.Append('%');
                result.Append(char.ToUpper(encoded[i + 1]));
                result.Append(char.ToUpper(encoded[i + 2]));
                i += 2;
            }
            else
            {
                result.Append(encoded[i]);
            }
        }
        return result.ToString();
    }
}

public class VNPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = System.Globalization.CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, System.Globalization.CompareOptions.Ordinal);
    }
}
