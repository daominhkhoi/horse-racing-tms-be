using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Shared.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file);
}
