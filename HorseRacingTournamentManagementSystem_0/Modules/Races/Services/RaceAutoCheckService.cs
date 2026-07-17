using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HorseRacingTournamentManagementSystem_0.Database;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Services
{
    /// <summary>
    /// Background Service kiểm tra mỗi 1 phút:
    /// - Nếu Race ở trạng thái "Registration Closed" và còn ≤15 phút trước RaceDateTime
    /// - Đếm số Horse đã Approved
    /// - Nếu đủ → chuyển sang "Ready To Start"
    /// - Nếu không đủ → chuyển sang "Cancelled"
    /// </summary>
    public class RaceAutoCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RaceAutoCheckService> _logger;

        public RaceAutoCheckService(IServiceProvider serviceProvider, ILogger<RaceAutoCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RaceAutoCheckService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckRacesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in RaceAutoCheckService.");
                }

                // Chờ 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("RaceAutoCheckService stopped.");
        }

        private async Task CheckRacesAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HorseRacingDbContext>();

            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(15);

            // Tìm các Race đang "Registration Closed" và còn ≤15 phút trước giờ chạy
            var racesToCheck = await context.Races
                .Where(r => r.Status == "Registration Closed"
                         && r.RaceDateTime != null
                         && r.RaceDateTime <= cutoff)
                .ToListAsync(stoppingToken);

            foreach (var race in racesToCheck)
            {
                var approvedCount = await context.RaceRegistrations
                    .CountAsync(r => r.RaceId == race.RaceId && r.Status == "Approved", stoppingToken);

                var minParticipants = race.MinParticipants ?? 4;

                if (approvedCount < minParticipants)
                {
                    race.Status = "Cancelled";
                    race.CancelReason = "Không đủ số lượng Horse tham gia.";
                    _logger.LogInformation(
                        "Race {RaceId} '{RaceName}' cancelled: {Approved}/{Min} approved horses.",
                        race.RaceId, race.RaceName, approvedCount, minParticipants);
                }
                else
                {
                    race.Status = "Ready To Start";
                    _logger.LogInformation(
                        "Race {RaceId} '{RaceName}' is ready to start: {Approved}/{Min} approved horses.",
                        race.RaceId, race.RaceName, approvedCount, minParticipants);
                }
            }

            if (racesToCheck.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
