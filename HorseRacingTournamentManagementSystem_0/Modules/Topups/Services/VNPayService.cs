using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using HorseRacingTournamentManagementSystem_0.Utils;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Topups.Services;

public class VNPayService : IVNPayService
{
    private readonly IConfiguration _configuration;
    private readonly HorseRacingDbContext _context;

    public VNPayService(IConfiguration configuration, HorseRacingDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public string CreatePaymentUrl(HttpContext context, double amount, int spectatorId)
    {
        string vnp_Returnurl = _configuration["VNPay:ReturnUrl"];
        string vnp_Url = _configuration["VNPay:Url"];
        string vnp_TmnCode = _configuration["VNPay:TmnCode"];
        string vnp_HashSecret = _configuration["VNPay:HashSecret"];
        
        var vnpay = new VNPayLibrary();
        
        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        
        // Amount must be multiplied by 100 for VNPay
        long vnpAmount = (long)(amount * 100);
        vnpay.AddRequestData("vnp_Amount", vnpAmount.ToString()); 
        
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        
        // Use 127.0.0.1 to avoid any IPv6 / formatting issues with VNPay sandbox
        vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
        
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", $"Nap_tien_cho_tai_khoan_{spectatorId}");
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
        
        // Unique transaction reference
        string txnRef = DateTime.Now.Ticks.ToString() + "_" + spectatorId;
        vnpay.AddRequestData("vnp_TxnRef", txnRef);

        string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
        return paymentUrl;
    }

    public async Task<string> ProcessIpn(IQueryCollection queryData)
    {
        var vnpay = new VNPayLibrary();
        
        foreach (var key in queryData.Keys)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, queryData[key]);
            }
        }
        
        string vnp_HashSecret = _configuration["VNPay:HashSecret"];
        string vnp_SecureHash = queryData["vnp_SecureHash"];
        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
        
        if (!checkSignature)
        {
            return "Invalid signature";
        }

        string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
        string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        double amount = Convert.ToDouble(vnpay.GetResponseData("vnp_Amount")) / 100;
        string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");

        var parts = vnp_TxnRef.Split('_');
        if (parts.Length < 2) return "Invalid TxnRef";

        if (!int.TryParse(parts[1], out int spectatorId))
        {
            return "Invalid SpectatorId in TxnRef";
        }

        var existingTxn = await _context.TopupTransactions.FirstOrDefaultAsync(t => t.VnpTxnRef == vnp_TxnRef);
        if (existingTxn != null)
        {
            return "Transaction already processed";
        }

        var spectator = await _context.SpectatorProfiles.FirstOrDefaultAsync(s => s.UserId == spectatorId);
        if (spectator == null)
        {
            return "Spectator not found";
        }

        double pointsAdded = amount / 1000.0;

        var txn = new TopupTransaction
        {
            SpectatorId = spectatorId,
            Amount = amount,
            PointsAdded = pointsAdded,
            VnpTxnRef = vnp_TxnRef,
            TransactionDate = DateTime.Now,
            Status = vnp_ResponseCode == "00" ? "Success" : "Failed"
        };

        if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
        {
            spectator.TotalPoints = (spectator.TotalPoints ?? 0) + pointsAdded;
        }

        _context.TopupTransactions.Add(txn);
        await _context.SaveChangesAsync();

        return vnp_ResponseCode == "00" ? "Success" : "Failed";
    }
}
