using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Horses.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Horses.Services
{
    public class HorseService : IHorseService
    {
        private readonly HorseRacingDbContext _context;

        public HorseService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<Horse> RegisterHorseAsync(CreateHorseDto createHorseDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var horse = new Horse
                {
                    OwnerId = createHorseDto.OwnerId,
                    HorseName = createHorseDto.HorseName,
                    Breed = createHorseDto.Breed,
                    Age = createHorseDto.Age,
                    Weight = createHorseDto.Weight,
                    Gender = createHorseDto.Gender,
                    HealthStatus = createHorseDto.HealthStatus,
                    ImageUrl = createHorseDto.ImageUrl,
                    Status = "Pending"
                };

                _context.Horses.Add(horse);
                await _context.SaveChangesAsync();

                var verification = new HorseVerification
                {
                    HorseId = horse.HorseId,
                    VerifyDate = DateTime.Now,
                    InspectionUrl = createHorseDto.InspectionUrl,
                    HealthCertUrl = createHorseDto.HealthCertUrl,
                    Result = "Pending"
                };

                _context.HorseVerifications.Add(verification);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return horse;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<IEnumerable<Horse>> GetHorsesByOwnerAsync(int ownerId)
        {
            return await _context.Horses
                .Include(h => h.HorseVerifications)
                .Where(h => h.OwnerId == ownerId)
                .OrderByDescending(h => h.HorseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Horse>> GetAllHorsesAsync()
        {
            return await _context.Horses
                .Include(h => h.HorseVerifications)
                .OrderByDescending(h => h.HorseId)
                .ToListAsync();
        }

        public async Task<Horse> VerifyHorseAsync(int horseId, VerifyHorseDto dto)
        {
            // Load horse WITHOUT navigation properties to avoid tracking conflicts
            var horse = await _context.Horses
                .FirstOrDefaultAsync(h => h.HorseId == horseId);

            if (horse == null)
                throw new Exception("Horse not found");

            // Update horse status directly
            horse.Status = dto.Status;

            // Find the latest pending verification record (load separately)
            var latestVerification = await _context.HorseVerifications
                .Where(v => v.HorseId == horseId && v.Result == "Pending")
                .OrderByDescending(v => v.VerifyId)
                .FirstOrDefaultAsync();

            if (latestVerification != null)
            {
                latestVerification.Result = dto.Status;
                latestVerification.Notes = dto.Notes;
                latestVerification.VerifyDate = DateTime.Now;
                // Set VerifiedBy to null to avoid FK constraint — 
                // VerifiedBy links to Users table and may not match any real user
                latestVerification.VerifiedBy = null;
            }

            await _context.SaveChangesAsync();

            // Return horse with verifications for the response
            return await _context.Horses
                .Include(h => h.HorseVerifications)
                .AsNoTracking()
                .FirstAsync(h => h.HorseId == horseId);
        }

        public async Task<bool> DeleteHorseAsync(int horseId)
        {
            var horse = await _context.Horses.FindAsync(horseId);
            if (horse == null) return false;

            horse.Status = "Retired";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateHorseStatusAsync(int horseId, string status)
        {
            var horse = await _context.Horses.FindAsync(horseId);
            if (horse == null) return false;

            horse.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RequestUpdateHorseAsync(int horseId, UpdateHorseDto dto)
        {
            var horse = await _context.Horses.FindAsync(horseId);
            if (horse == null) return false;

            var verification = new HorseVerification
            {
                HorseId = horseId,
                VerifyDate = DateTime.Now,
                Result = "Update_Pending",
                Notes = JsonSerializer.Serialize(dto)
            };

            _context.HorseVerifications.Add(verification);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveUpdateHorseAsync(int horseId, int adminId, string notes)
        {
            var verification = await _context.HorseVerifications
                .Where(v => v.HorseId == horseId && v.Result == "Update_Pending")
                .OrderByDescending(v => v.VerifyId)
                .FirstOrDefaultAsync();

            if (verification == null) return false;

            var horse = await _context.Horses.FindAsync(horseId);
            if (horse == null) return false;

            var updatedData = JsonSerializer.Deserialize<UpdateHorseDto>(verification.Notes ?? "{}");
            if (updatedData != null)
            {
                horse.HorseName = updatedData.HorseName;
                horse.Breed = updatedData.Breed;
                horse.Age = updatedData.Age;
                horse.Weight = updatedData.Weight;
                horse.Gender = updatedData.Gender;
                horse.HealthStatus = updatedData.HealthStatus;
                if (!string.IsNullOrEmpty(updatedData.ImageUrl))
                {
                    horse.ImageUrl = updatedData.ImageUrl;
                }
                if (!string.IsNullOrEmpty(updatedData.Status))
                {
                    horse.Status = updatedData.Status;
                }
            }

            verification.Result = "Update_Approved";
            verification.VerifiedBy = adminId;
            verification.VerifyDate = DateTime.Now;
            
            // Optionally, we could append admin notes to the JSON or a separate field, 
            // but we'll leave the Notes as the JSON string since we don't want to lose the update data history.

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectUpdateHorseAsync(int horseId, int adminId, string notes)
        {
            var verification = await _context.HorseVerifications
                .Where(v => v.HorseId == horseId && v.Result == "Update_Pending")
                .OrderByDescending(v => v.VerifyId)
                .FirstOrDefaultAsync();

            if (verification == null) return false;

            verification.Result = "Update_Rejected";
            verification.VerifiedBy = adminId;
            verification.VerifyDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
