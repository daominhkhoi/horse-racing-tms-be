using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Topups.Services;

public interface IVNPayService
{
    string CreatePaymentUrl(HttpContext context, double amount, int spectatorId);
    Task<string> ProcessIpn(IQueryCollection queryData);
}
