using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Invitations.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Invitations.Interfaces;
using HorseRacingTournamentManagementSystem_0.Modules.Races.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitations.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly HorseRacingDbContext _context;
        private readonly IRaceRegistrationService _raceRegistrationService;

        public InvitationService(HorseRacingDbContext context, IRaceRegistrationService raceRegistrationService)
        {
            _context = context;
            _raceRegistrationService = raceRegistrationService;
        }

        public async Task<InvitationResponse> SendInvitationAsync(int ownerId, SendInvitationRequest request)
        {
            var horse = await _context.Horses.FirstOrDefaultAsync(h => h.HorseId == request.HorseId && h.OwnerId == ownerId);
            if (horse == null)
            {
                throw new Exception("Horse not found or you don't have ownership.");
            }

            var jockey = await _context.JockeyProfiles.FirstOrDefaultAsync(j => j.UserId == request.JockeyId);
            if (jockey == null)
            {
                throw new Exception("Jockey not found.");
            }

            var tour = await _context.Tournaments.FirstOrDefaultAsync(t => t.TourId == request.TourId);
            if (tour == null)
            {
                throw new Exception("Tournament not found.");
            }

            if (!await _raceRegistrationService.HasApprovedRegistrationAsync(ownerId, request.HorseId, request.TourId))
                throw new Exception("Horse registration must be approved by an admin before inviting a jockey.");

            if (await _context.Invitations.AnyAsync(i => i.HorseId == request.HorseId && i.TourId == request.TourId && (i.Status == "Accepted" || i.Status == "AcceptedPendingAdmin")))
                throw new Exception("This horse already has a jockey confirmation awaiting or approved by admin.");

            if (await _context.Invitations.AnyAsync(i => i.JockeyId == request.JockeyId && i.TourId == request.TourId && (i.Status == "Accepted" || i.Status == "AcceptedPendingAdmin")))
                throw new Exception("This jockey already confirmed another horse in this tournament.");

            var existingInvite = await _context.Invitations
                .FirstOrDefaultAsync(i => i.JockeyId == request.JockeyId 
                                       && i.HorseId == request.HorseId 
                                       && i.TourId == request.TourId 
                                       && i.Status == "Pending");
            if (existingInvite != null)
            {
                throw new Exception("A pending invitation has already been sent to this Jockey for this horse and tournament.");
            }

            var invitation = new Invitation
            {
                OwnerId = ownerId,
                JockeyId = request.JockeyId,
                HorseId = request.HorseId,
                TourId = request.TourId,
                Message = request.Message,
                Status = "Pending",
                SentAt = DateTime.UtcNow
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Load related entities for response
            var newInvite = await _context.Invitations
                .Include(i => i.Owner).ThenInclude(o => o.User)
                .Include(i => i.Jockey).ThenInclude(j => j.User)
                .Include(i => i.Horse)
                .Include(i => i.Tour)
                .FirstOrDefaultAsync(i => i.InviteId == invitation.InviteId);

            return MapToResponse(newInvite);
        }

        public async Task<bool> CancelInvitationAsync(int ownerId, int inviteId)
        {
            var invite = await _context.Invitations.FirstOrDefaultAsync(i => i.InviteId == inviteId && i.OwnerId == ownerId);
            if (invite == null)
            {
                throw new Exception("Invitation not found or you don't have permission.");
            }

            if (invite.Status != "Pending")
            {
                throw new Exception("Only pending invitations can be canceled.");
            }

            invite.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<InvitationResponse>> GetMyInvitationsAsync(int jockeyId, string? status = null)
        {
            var query = _context.Invitations
                .Include(i => i.Owner).ThenInclude(o => o.User)
                .Include(i => i.Jockey).ThenInclude(j => j.User)
                .Include(i => i.Horse)
                .Include(i => i.Tour)
                .Where(i => i.JockeyId == jockeyId);

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "AutoCancelled")
                {
                    query = query.Where(i => i.Status == "AutoCancelled" || i.Status == "Auto-Canceled");
                }
                else
                {
                    query = query.Where(i => i.Status == status);
                }
            }

            var invites = await query.OrderByDescending(i => i.SentAt).ToListAsync();
            return invites.Select(MapToResponse);
        }

        public async Task<IEnumerable<InvitationResponse>> GetSentInvitationsAsync(int ownerId, string? status = null)
        {
            var query = _context.Invitations
                .Include(i => i.Owner).ThenInclude(o => o.User)
                .Include(i => i.Jockey).ThenInclude(j => j.User)
                .Include(i => i.Horse)
                .Include(i => i.Tour)
                .Where(i => i.OwnerId == ownerId);

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "AutoCancelled")
                {
                    query = query.Where(i => i.Status == "AutoCancelled" || i.Status == "Auto-Canceled");
                }
                else
                {
                    query = query.Where(i => i.Status == status);
                }
            }

            var invites = await query.OrderByDescending(i => i.SentAt).ToListAsync();
            return invites.Select(MapToResponse);
        }

        public async Task<bool> RespondToInvitationAsync(int jockeyId, int inviteId, bool isAccepted)
        {
            var invite = await _context.Invitations.FirstOrDefaultAsync(i => i.InviteId == inviteId && i.JockeyId == jockeyId);
            if (invite == null)
            {
                throw new Exception("Invitation not found or you don't have permission.");
            }

            if (invite.Status != "Pending")
            {
                throw new Exception("This invitation has already been processed.");
            }

            if (isAccepted)
            {
                var jockeyAlreadyCommitted = await _context.Invitations.AnyAsync(i =>
                    i.TourId == invite.TourId &&
                    i.JockeyId == jockeyId &&
                    i.InviteId != inviteId &&
                    (i.Status == "AcceptedPendingAdmin" || i.Status == "Accepted"));
                if (jockeyAlreadyCommitted)
                    throw new Exception("You already confirmed another horse in this tournament.");
            }

            invite.Status = isAccepted ? "AcceptedPendingAdmin" : "Rejected";

            if (isAccepted)
            {
                // One horse can only have one jockey, and one jockey can only
                // ride one horse in the same tournament. Cancel both kinds of
                // conflicting invitations as soon as a jockey confirms.
                var otherPendingInvites = await _context.Invitations
                    .Where(i => i.TourId == invite.TourId
                             && i.Status == "Pending" 
                             && i.InviteId != inviteId
                             && (i.HorseId == invite.HorseId || i.JockeyId == jockeyId))
                    .ToListAsync();

                foreach (var otherInvite in otherPendingInvites)
                {
                    otherInvite.Status = "AutoCancelled";
                }

            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReviewAcceptedInvitationAsync(int inviteId, bool approved)
        {
            var invite = await _context.Invitations.FirstOrDefaultAsync(i => i.InviteId == inviteId)
                ?? throw new Exception("Invitation not found.");
            if (invite.Status != "AcceptedPendingAdmin")
                throw new Exception("Only a jockey-confirmed invitation can be reviewed.");

            invite.Status = approved ? "Accepted" : "AdminRejected";
            if (approved)
                await _raceRegistrationService.SyncAcceptedInvitationAsync(invite.HorseId, invite.JockeyId, invite.TourId);

            await _context.SaveChangesAsync();
            return true;
        }

        private static InvitationResponse MapToResponse(Invitation invite)
        {
            return new InvitationResponse
            {
                InviteId = invite.InviteId,
                OwnerId = invite.OwnerId,
                OwnerName = invite.Owner?.User?.FullName ?? "Unknown Owner",
                JockeyId = invite.JockeyId,
                JockeyName = invite.Jockey?.User?.FullName ?? "Unknown Jockey",
                HorseId = invite.HorseId,
                HorseName = invite.Horse?.HorseName ?? "Unknown Horse",
                TourId = invite.TourId,
                TourName = invite.Tour?.TourName ?? "Unknown Tournament",
                Message = invite.Message,
                Status = invite.Status,
                // SQL Server `datetime` does not retain DateTimeKind. Mark the
                // stored UTC value explicitly so JSON includes the `Z` suffix.
                SentAt = invite.SentAt.HasValue
                    ? DateTime.SpecifyKind(invite.SentAt.Value, DateTimeKind.Utc)
                    : null
            };
        }
    }
}
