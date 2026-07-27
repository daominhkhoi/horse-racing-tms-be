using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Jockey.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Jockey.Services
{
    public class JockeyService : IJockeyService
    {
        private readonly HorseRacingDbContext _context;

        public JockeyService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RequestUpdateProfileAsync(int jockeyId, UpdateJockeyDto dto)
        {
            var profile = await _context.JockeyProfiles
                .FirstOrDefaultAsync(j => j.UserId == jockeyId);

            if (profile == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                bool phoneExists = await _context.AdminProfiles.AnyAsync(p => p.Phone == dto.Phone && p.UserId != jockeyId) ||
                                   await _context.JockeyProfiles.AnyAsync(p => (p.Phone == dto.Phone || p.PendingPhone == dto.Phone) && p.UserId != jockeyId) ||
                                   await _context.OwnerProfiles.AnyAsync(p => p.Phone == dto.Phone && p.UserId != jockeyId) ||
                                   await _context.RefereeProfiles.AnyAsync(p => p.Phone == dto.Phone && p.UserId != jockeyId) ||
                                   await _context.SpectatorProfiles.AnyAsync(p => p.Phone == dto.Phone && p.UserId != jockeyId);

                if (phoneExists)
                {
                    throw new System.Exception("Phone number is already in use by another account.");
                }
            }

            if (dto.Phone != null)         profile.PendingPhone = dto.Phone;
            if (dto.Avatar != null)        profile.PendingAvatar = dto.Avatar;
            if (dto.ExperienceYear.HasValue) profile.PendingExperienceYear = dto.ExperienceYear;

            profile.UpdateStatus = "Pending";
            profile.UpdateRequestedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReviewUpdateRequestAsync(int jockeyId, ReviewJockeyDto dto)
        {
            var profile = await _context.JockeyProfiles
                .FirstOrDefaultAsync(j => j.UserId == jockeyId && j.UpdateStatus == "Pending");

            if (profile == null) return false;

            if (dto.IsApproved)
            {
                if (profile.PendingPhone != null)
                    profile.Phone = profile.PendingPhone;

                if (profile.PendingAvatar != null)
                    profile.Avatar = profile.PendingAvatar;

                if (profile.PendingExperienceYear.HasValue)
                    profile.ExperienceYear = profile.PendingExperienceYear;

                profile.UpdateStatus = "Approved";
            }
            else
            {
                profile.UpdateStatus = "Rejected";
            }

            profile.PendingPhone = null;
            profile.PendingAvatar = null;
            profile.PendingExperienceYear = null;

            profile.ReviewedBy = dto.ReviewedBy;
            profile.ReviewNotes = dto.Notes;
            profile.ReviewedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<JockeyProfile>> GetAllJockeysPublicAsync()
        {
            return await _context.JockeyProfiles
                .Include(j => j.User)
                .OrderByDescending(j => j.UserId)
                .ToListAsync();
        }

        public async Task<IEnumerable<JockeyProfile>> GetAvailableJockeysForRaceAsync(int raceId)
        {
            var race = await _context.Races
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RaceId == raceId)
                ?? throw new Exception("Race not found.");

            // Pending invitations do not reserve a jockey. Once accepted, the
            // jockey is busy only in the race of the registered horse.
            var acceptedInvites = await _context.Invitations
                .Where(i => i.TourId == race.TourId
                    && (i.Status == "Accepted" || i.Status == "AcceptedPendingAdmin")
                    && _context.RaceRegistrations.Any(r =>
                        r.RaceId == raceId
                        && r.HorseId == i.HorseId
                        && r.Status == "Approved"))
                .Select(i => i.JockeyId)
                .ToListAsync();

            var participantJockeys = await _context.RaceParticipants
                .Where(p => p.RaceId == raceId
                    && p.ParticipationStatus != "Rejected"
                    && p.ParticipationStatus != "Cancelled")
                .Select(p => p.JockeyId)
                .ToListAsync();

            var busyIds = acceptedInvites
                .Concat(participantJockeys)
                .Distinct()
                .ToList();

            return await _context.JockeyProfiles
                .Include(j => j.User)
                .Where(j => j.User.IsActive == true && !busyIds.Contains(j.UserId))
                .OrderByDescending(j => j.UserId)
                .ToListAsync();
        }
    }
}
