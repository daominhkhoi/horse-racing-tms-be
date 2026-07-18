using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.Services;

public class RaceRegistrationService : IRaceRegistrationService
{
    private readonly HorseRacingDbContext _context;
    public RaceRegistrationService(HorseRacingDbContext context) => _context = context;

    public async Task<RaceRegistrationResponse> RegisterHorseAsync(int ownerId, int raceId, RegisterHorseRequest request)
    {
        var race = await _context.Races.FindAsync(raceId) ?? throw new Exception("Race not found.");
        if (race.Status != "Open Registration" && race.Status != "Upcoming") throw new Exception("Race registration is not open.");
        if (race.RaceDateTime.HasValue && race.RaceDateTime.Value <= DateTime.Now) throw new Exception("Cannot register for a race that has started.");

        var horse = await _context.Horses.FirstOrDefaultAsync(h => h.HorseId == request.HorseId && h.OwnerId == ownerId);
        if (horse == null) throw new Exception("Horse not found or you do not own this horse.");
        if (horse.Status != "Approved") throw new Exception("Only horses with an approved profile can be registered.");

        var tournamentRaceIds = _context.Races.Where(r => r.TourId == race.TourId).Select(r => r.RaceId);
        if (await _context.RaceParticipants.AnyAsync(p => tournamentRaceIds.Contains(p.RaceId) && p.HorseId == request.HorseId))
            throw new Exception("This horse has already been assigned to a lane in this tournament.");
        if (await _context.RaceRegistrations.AnyAsync(r => r.HorseId == request.HorseId && r.Race.TourId == race.TourId && r.Status != "Rejected"))
            throw new Exception("This horse already has a registration in this tournament.");

        var registration = new RaceRegistration { RaceId = raceId, HorseId = request.HorseId, OwnerId = ownerId, Status = "Pending", RegisterTime = DateTime.Now };
        _context.RaceRegistrations.Add(registration);
        await _context.SaveChangesAsync();
        return await GetRegistrationAsync(registration.RegistrationId);
    }

    public async Task<IEnumerable<AvailableRaceHorseResponse>> GetAvailableHorsesAsync(int ownerId, int raceId)
    {
        var race = await _context.Races.FindAsync(raceId) ?? throw new Exception("Race not found.");
        var tournamentRaceIds = _context.Races.Where(r => r.TourId == race.TourId).Select(r => r.RaceId);
        var assignedIds = _context.RaceParticipants.Where(p => tournamentRaceIds.Contains(p.RaceId)).Select(p => p.HorseId);
        var registeredIds = _context.RaceRegistrations.Where(r => r.Race.TourId == race.TourId && r.Status != "Rejected").Select(r => r.HorseId);
        return await _context.Horses
            .Where(h => h.OwnerId == ownerId && h.Status == "Approved" && !assignedIds.Contains(h.HorseId) && !registeredIds.Contains(h.HorseId))
            .OrderBy(h => h.HorseName)
            .Select(h => new AvailableRaceHorseResponse { HorseId = h.HorseId, HorseName = h.HorseName, Breed = h.Breed, ImageUrl = h.ImageUrl })
            .ToListAsync();
    }

    public async Task<RaceRegistrationResponse> ReviewRegistrationAsync(int adminId, int registrationId, bool approved, string? reason)
    {
        var registration = await _context.RaceRegistrations.Include(r => r.Race).FirstOrDefaultAsync(r => r.RegistrationId == registrationId) ?? throw new Exception("Registration not found.");
        if (registration.Race.Status != "Open Registration" && registration.Race.Status != "Upcoming") throw new Exception("Registration can only be reviewed while registration is open.");
        if (registration.Status != "Pending") throw new Exception("This registration has already been reviewed.");
        if (approved)
        {
            var approvedCount = await _context.RaceRegistrations.CountAsync(r => r.RaceId == registration.RaceId && r.Status == "Approved");
            var assignedCount = await _context.RaceParticipants.CountAsync(p => p.RaceId == registration.RaceId);
            if (approvedCount + assignedCount >= registration.Race.MaxParticipants) throw new Exception("The race has reached its maximum number of participants.");
        }
        registration.Status = approved ? "Approved" : "Rejected";
        registration.ReviewedBy = adminId;
        registration.ReviewedAt = DateTime.Now;
        registration.RejectReason = approved ? null : reason;
        await _context.SaveChangesAsync();
        return await GetRegistrationAsync(registrationId);
    }

    public async Task<IEnumerable<RaceRegistrationResponse>> GetRegistrationsAsync(int raceId, string? status)
    {
        var query = RegistrationQuery().Where(r => r.RaceId == raceId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(r => r.Status == status);
        var responses = (await query.OrderByDescending(r => r.RegisterTime).ToListAsync()).Select(Map).ToList();
        await EnrichInvitationsAsync(responses);
        return responses;
    }

    public async Task<IEnumerable<RaceRegistrationResponse>> GetMyRegistrationsAsync(int ownerId)
    {
        var responses = (await RegistrationQuery().Where(r => r.OwnerId == ownerId).OrderByDescending(r => r.RegisterTime).ToListAsync()).Select(Map).ToList();
        await EnrichInvitationsAsync(responses);
        return responses;
    }

    public async Task<RaceRegistrationSummaryResponse> GetSummaryAsync(int raceId)
    {
        var race = await _context.Races.FindAsync(raceId) ?? throw new Exception("Race not found.");
        var registrations = _context.RaceRegistrations.Where(r => r.RaceId == raceId);
        var accepted = await registrations.CountAsync(r => r.Status == "Approved" && _context.Invitations.Any(i => i.HorseId == r.HorseId && i.TourId == race.TourId && i.Status == "Accepted"));
        return new RaceRegistrationSummaryResponse { RaceId = raceId, RaceStatus = race.Status, MinParticipants = race.MinParticipants, MaxParticipants = race.MaxParticipants, PendingCount = await registrations.CountAsync(r => r.Status == "Pending"), ApprovedCount = await registrations.CountAsync(r => r.Status == "Approved"), AcceptedJockeyCount = accepted, CancelReason = race.CancelReason };
    }

    public async Task SetRegistrationStatusAsync(int raceId, string status)
    {
        var race = await _context.Races.FindAsync(raceId) ?? throw new Exception("Race not found.");
        if (status == "Open Registration")
        {
            if (race.Status != "Draft" && race.Status != "Upcoming" && race.Status != "Registration Closed") throw new Exception("Race registration cannot be opened in its current status.");
            race.CancelReason = null;
        }
        else if (status == "Registration Closed")
        {
            if (race.Status != "Open Registration" && race.Status != "Upcoming") throw new Exception("Only an open race can close registration.");
        }
        else throw new Exception("Invalid registration status.");
        race.Status = status;
        await _context.SaveChangesAsync();
    }

    public async Task StartRaceAsync(int raceId)
    {
        var race = await _context.Races.FindAsync(raceId) ?? throw new Exception("Race not found.");
        if (race.Status != "Ready To Start") throw new Exception("Race is not ready to start.");
        race.Status = "Racing";
        await _context.SaveChangesAsync();
    }

    public Task<bool> HasApprovedRegistrationAsync(int ownerId, int horseId, int tourId) =>
        _context.RaceRegistrations.AnyAsync(r => r.OwnerId == ownerId && r.HorseId == horseId && r.Status == "Approved" && r.Race.TourId == tourId);

    public async Task SyncAcceptedInvitationAsync(int horseId, int jockeyId, int tourId)
    {
        var registration = await _context.RaceRegistrations.Include(r => r.Race).FirstOrDefaultAsync(r => r.HorseId == horseId && r.Status == "Approved" && r.Race.TourId == tourId);
        if (registration == null) return;
        if (!await _context.RaceParticipants.AnyAsync(p => p.RaceId == registration.RaceId && p.HorseId == horseId))
        {
            var usedLanes = await _context.RaceParticipants.Where(p => p.RaceId == registration.RaceId && p.LaneNumber.HasValue).Select(p => p.LaneNumber!.Value).ToListAsync();
            var lane = Enumerable.Range(1, registration.Race.MaxParticipants).FirstOrDefault(n => !usedLanes.Contains(n));
            _context.RaceParticipants.Add(new RaceParticipant { RaceId = registration.RaceId, HorseId = horseId, JockeyId = jockeyId, LaneNumber = lane == 0 ? null : lane, ParticipationStatus = "Approved" });
            await _context.SaveChangesAsync();
        }
    }

    private IQueryable<RaceRegistration> RegistrationQuery() => _context.RaceRegistrations.Include(r => r.Race).ThenInclude(r => r.Tour).Include(r => r.Horse).Include(r => r.Owner).ThenInclude(o => o.User);
    private async Task<RaceRegistrationResponse> GetRegistrationAsync(int id) => Map(await RegistrationQuery().FirstAsync(r => r.RegistrationId == id));
    private async Task EnrichInvitationsAsync(List<RaceRegistrationResponse> responses)
    {
        if (responses.Count == 0) return;
        var horseIds = responses.Select(r => r.HorseId).Distinct().ToList();
        var tourIds = responses.Select(r => r.TourId).Distinct().ToList();
        var invitations = await _context.Invitations
            .Include(i => i.Jockey).ThenInclude(j => j.User)
            .Where(i => horseIds.Contains(i.HorseId) && tourIds.Contains(i.TourId) &&
                (i.Status == "AcceptedPendingAdmin" || i.Status == "Accepted"))
            .OrderByDescending(i => i.Status == "Accepted")
            .ThenByDescending(i => i.SentAt)
            .ToListAsync();
        foreach (var response in responses)
        {
            var invite = invitations.FirstOrDefault(i => i.HorseId == response.HorseId && i.TourId == response.TourId);
            if (invite == null) continue;
            response.InviteId = invite.InviteId;
            response.JockeyName = invite.Jockey?.User?.FullName;
            response.InvitationStatus = invite.Status;
        }
    }
    private static RaceRegistrationResponse Map(RaceRegistration r) => new() { RegistrationId = r.RegistrationId, RaceId = r.RaceId, RaceName = r.Race.RaceName, TourId = r.Race.TourId, TourName = r.Race.Tour?.TourName, HorseId = r.HorseId, HorseName = r.Horse?.HorseName, OwnerId = r.OwnerId, OwnerName = r.Owner?.User?.FullName, Status = r.Status, RegisterTime = r.RegisterTime, ReviewedAt = r.ReviewedAt, RejectReason = r.RejectReason };
}
