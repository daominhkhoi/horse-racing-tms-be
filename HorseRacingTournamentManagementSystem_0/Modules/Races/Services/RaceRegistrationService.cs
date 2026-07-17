using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Services
{
    public class RaceRegistrationService : IRaceRegistrationService
    {
        private readonly HorseRacingDbContext _context;

        public RaceRegistrationService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<RaceRegistrationDto> RegisterHorseAsync(int ownerId, int raceId, int horseId)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null)
                throw new Exception("Race not found.");

            if (race.Status != "Open Registration")
                throw new Exception("Race is not open for registration.");

            var horse = await _context.Horses.FirstOrDefaultAsync(h => h.HorseId == horseId && h.OwnerId == ownerId);
            if (horse == null)
                throw new Exception("Horse not found or you don't own this horse.");

            // Check if horse already registered
            var existing = await _context.RaceRegistrations
                .FirstOrDefaultAsync(r => r.RaceId == raceId && r.HorseId == horseId);
            if (existing != null)
                throw new Exception("This horse is already registered for this race.");

            // Check max participants
            var approvedCount = await _context.RaceRegistrations
                .CountAsync(r => r.RaceId == raceId && r.Status == "Approved");
            if (race.MaxParticipants.HasValue && approvedCount >= race.MaxParticipants.Value)
                throw new Exception("Maximum number of participants reached for this race.");

            var registration = new RaceRegistration
            {
                RaceId = raceId,
                HorseId = horseId,
                OwnerId = ownerId,
                Status = "Pending",
                RegisteredAt = DateTime.UtcNow
            };

            _context.RaceRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            // Reload with includes
            var result = await _context.RaceRegistrations
                .Include(r => r.Horse)
                .Include(r => r.Owner).ThenInclude(o => o.User)
                .Include(r => r.Race)
                .FirstOrDefaultAsync(r => r.RegistrationId == registration.RegistrationId);

            return MapToDto(result!);
        }

        public async Task<bool> ApproveRegistrationAsync(int registrationId)
        {
            var reg = await _context.RaceRegistrations
                .Include(r => r.Race)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);

            if (reg == null)
                throw new Exception("Registration not found.");

            if (reg.Status != "Pending")
                throw new Exception("Only pending registrations can be approved.");

            var race = reg.Race;
            if (race.Status != "Open Registration" && race.Status != "Registration Closed")
                throw new Exception("Race is not in a valid state for approving registrations.");

            // Check max participants
            var approvedCount = await _context.RaceRegistrations
                .CountAsync(r => r.RaceId == reg.RaceId && r.Status == "Approved");
            if (race.MaxParticipants.HasValue && approvedCount >= race.MaxParticipants.Value)
                throw new Exception("Maximum number of participants reached.");

            reg.Status = "Approved";
            reg.ReviewedAt = DateTime.UtcNow;

            // Create Race_Participant with JockeyId = null
            var participant = new RaceParticipant
            {
                RaceId = reg.RaceId,
                HorseId = reg.HorseId,
                JockeyId = null,
                ParticipationStatus = "Registered"
            };

            _context.RaceParticipants.Add(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectRegistrationAsync(int registrationId, string? note)
        {
            var reg = await _context.RaceRegistrations.FindAsync(registrationId);
            if (reg == null)
                throw new Exception("Registration not found.");

            if (reg.Status != "Pending")
                throw new Exception("Only pending registrations can be rejected.");

            reg.Status = "Rejected";
            reg.ReviewedAt = DateTime.UtcNow;
            reg.ReviewNote = note;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RaceRegistrationDto>> GetRegistrationsByRaceAsync(int raceId)
        {
            var registrations = await _context.RaceRegistrations
                .Include(r => r.Horse)
                .Include(r => r.Owner).ThenInclude(o => o.User)
                .Include(r => r.Race)
                .Where(r => r.RaceId == raceId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        public async Task<List<RaceRegistrationDto>> GetMyRegistrationsAsync(int ownerId)
        {
            var registrations = await _context.RaceRegistrations
                .Include(r => r.Horse)
                .Include(r => r.Owner).ThenInclude(o => o.User)
                .Include(r => r.Race)
                .Where(r => r.OwnerId == ownerId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        public async Task<bool> OpenRegistrationAsync(int raceId)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null)
                throw new Exception("Race not found.");

            if (race.Status != "Upcoming" && race.Status != "Draft")
                throw new Exception("Race must be in 'Upcoming' or 'Draft' status to open registration.");

            race.Status = "Open Registration";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseRegistrationAsync(int raceId)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null)
                throw new Exception("Race not found.");

            if (race.Status != "Open Registration")
                throw new Exception("Race must be in 'Open Registration' status to close registration.");

            race.Status = "Registration Closed";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RaceStatusDto> GetRaceStatusAsync(int raceId)
        {
            var race = await _context.Races.FindAsync(raceId);
            if (race == null)
                throw new Exception("Race not found.");

            var approvedCount = await _context.RaceRegistrations
                .CountAsync(r => r.RaceId == raceId && r.Status == "Approved");
            var pendingCount = await _context.RaceRegistrations
                .CountAsync(r => r.RaceId == raceId && r.Status == "Pending");
            var rejectedCount = await _context.RaceRegistrations
                .CountAsync(r => r.RaceId == raceId && r.Status == "Rejected");

            return new RaceStatusDto
            {
                RaceId = race.RaceId,
                RaceName = race.RaceName,
                Status = race.Status,
                RaceDateTime = race.RaceDateTime,
                MinParticipants = race.MinParticipants,
                MaxParticipants = race.MaxParticipants,
                ApprovedCount = approvedCount,
                PendingCount = pendingCount,
                RejectedCount = rejectedCount,
                CancelReason = race.CancelReason,
                Distance = race.Distance,
                RewardRatio = race.RewardRatio
            };
        }

        public async Task<List<ApprovedParticipantDto>> GetApprovedParticipantsAsync(int raceId)
        {
            var participants = await _context.RaceParticipants
                .Include(p => p.Horse).ThenInclude(h => h.Owner).ThenInclude(o => o.User)
                .Include(p => p.Jockey).ThenInclude(j => j.User)
                .Where(p => p.RaceId == raceId)
                .ToListAsync();

            return participants.Select(p => new ApprovedParticipantDto
            {
                ParticipantId = p.ParticipantId,
                HorseId = p.HorseId,
                HorseName = p.Horse?.HorseName,
                HorseImageUrl = p.Horse?.ImageUrl,
                JockeyId = p.JockeyId,
                JockeyName = p.Jockey?.User?.FullName,
                JockeyAvatar = p.Jockey?.Avatar,
                LaneNumber = p.LaneNumber,
                ParticipationStatus = p.ParticipationStatus,
                OwnerId = p.Horse?.OwnerId ?? 0,
                OwnerName = p.Horse?.Owner?.User?.FullName
            }).ToList();
        }

        public async Task<bool> ApproveJockeyAsync(int participantId)
        {
            var participant = await _context.RaceParticipants.FindAsync(participantId);
            if (participant == null)
                throw new Exception("Participant not found.");

            if (participant.JockeyId == null)
                throw new Exception("No jockey has been assigned to this participant yet.");

            if (participant.ParticipationStatus != "Confirmed")
                throw new Exception("Jockey assignment must be in 'Confirmed' status (accepted by jockey) to be approved by Admin.");

            participant.ParticipationStatus = "Approved";
            await _context.SaveChangesAsync();
            return true;
        }

        private static RaceRegistrationDto MapToDto(RaceRegistration reg)

        {
            return new RaceRegistrationDto
            {
                RegistrationId = reg.RegistrationId,
                RaceId = reg.RaceId,
                RaceName = reg.Race?.RaceName,
                TourId = reg.Race != null ? reg.Race.TourId : 0,
                HorseId = reg.HorseId,

                HorseName = reg.Horse?.HorseName,
                HorseImageUrl = reg.Horse?.ImageUrl,
                OwnerId = reg.OwnerId,
                OwnerName = reg.Owner?.User?.FullName ?? "Unknown",
                Status = reg.Status,
                RegisteredAt = reg.RegisteredAt,
                ReviewedAt = reg.ReviewedAt,
                ReviewNote = reg.ReviewNote
            };
        }
    }
}
