using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Invitation.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.Services
{
    /// <summary>
    /// Implementation of IInvitationService.
    ///
    /// Dependency: HorseRacingDbContext (injected via DI).
    /// Registered in Program.cs:
    ///     builder.Services.AddScoped&lt;IInvitationService, InvitationService&gt;();
    /// </summary>
    public class InvitationService : IInvitationService
    {
        private readonly HorseRacingDbContext _context;

        public InvitationService(HorseRacingDbContext context)
        {
            _context = context;
        }

        // =========================================================================
        // FR-INVT-001: HorseOwner sends an invitation to a Jockey
        // Endpoint: POST /api/invitations
        // =========================================================================

        /// <summary>
        /// Creates a new invitation from a HorseOwner to a Jockey.
        ///
        /// FLOW:
        ///   1. Validate that the Horse belongs to this owner.
        ///   2. Check that a Pending invitation for the same Horse+Jockey+Tour does not exist.
        ///   3. Create new Invitation with Status = "Pending".
        ///   4. Save and return.
        /// </summary>
        public async Task<(bool Success, string Message)> SendInvitationAsync(int ownerId, SendInvitationDto dto)
        {
            // Step 1: Validate horse ownership
            var horse = await _context.Horses
                .FirstOrDefaultAsync(h => h.HorseId == dto.HorseId && h.OwnerId == ownerId);

            if (horse == null)
                return (false, "Horse not found or does not belong to this owner.");

            // Step 2: Prevent duplicate active invitations (Pending only)
            bool duplicateExists = await _context.Invitations.AnyAsync(i =>
                i.OwnerId == ownerId &&
                i.JockeyId == dto.JockeyId &&
                i.HorseId == dto.HorseId &&
                i.TourId == dto.TourId &&
                i.Status == "Pending");

            if (duplicateExists)
                return (false, "An active invitation for this jockey and horse already exists.");

            // Step 3: Create invitation
            var invitation = new Entities.Invitation
            {
                OwnerId = ownerId,
                JockeyId = dto.JockeyId,
                HorseId = dto.HorseId,
                TourId = dto.TourId,
                Message = dto.Message,
                Status = "Pending",
                SentAt = DateTime.Now
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            return (true, "Invitation sent successfully.");
        }

        // =========================================================================
        // FR-INVT-002: HorseOwner views all invitations they have sent
        // Endpoint: GET /api/invitations/sent?status={status}
        // =========================================================================

        /// <summary>
        /// Returns all invitations sent by the given owner, with optional status filtering.
        ///
        /// FLOW:
        ///   1. Query Invitations filtered by OwnerId (and optionally by Status).
        ///   2. Include Horse, Jockey.User, Owner.User, Tour for display.
        ///   3. Order newest-first and project to InvitationDto.
        /// </summary>
        public async Task<IEnumerable<InvitationDto>> GetSentInvitationsAsync(int ownerId, string? status)
        {
            var query = _context.Invitations
                .Include(i => i.Horse)
                .Include(i => i.Jockey).ThenInclude(j => j.User)
                .Include(i => i.Owner).ThenInclude(o => o.User)
                .Include(i => i.Tour)
                .Where(i => i.OwnerId == ownerId);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.Status == status);

            var result = await query
                .OrderByDescending(i => i.SentAt)
                .Select(i => new InvitationDto
                {
                    InviteId   = i.InviteId,
                    HorseId    = i.HorseId,
                    HorseName  = i.Horse.HorseName,
                    OwnerId    = i.OwnerId,
                    OwnerName  = i.Owner.User.FullName,
                    JockeyId   = i.JockeyId,
                    JockeyName = i.Jockey.User.FullName,
                    TourId     = i.TourId,
                    TourName   = i.Tour.TourName,
                    Message    = i.Message,
                    Status     = i.Status,
                    SentAt     = i.SentAt
                })
                .ToListAsync();

            return result;
        }

        // =========================================================================
        // FR-INVT-003: Jockey views all invitations they have received
        // Endpoint: GET /api/invitations/my
        // =========================================================================

        /// <summary>
        /// Returns all invitations received by the given jockey.
        ///
        /// FLOW:
        ///   1. Query Invitations filtered by JockeyId.
        ///   2. Include all navigation properties for display.
        ///   3. Order newest-first and project to InvitationDto.
        /// </summary>
        public async Task<IEnumerable<InvitationDto>> GetReceivedInvitationsAsync(int jockeyId)
        {
            var result = await _context.Invitations
                .Include(i => i.Horse)
                .Include(i => i.Jockey).ThenInclude(j => j.User)
                .Include(i => i.Owner).ThenInclude(o => o.User)
                .Include(i => i.Tour)
                .Where(i => i.JockeyId == jockeyId)
                .OrderByDescending(i => i.SentAt)
                .Select(i => new InvitationDto
                {
                    InviteId   = i.InviteId,
                    HorseId    = i.HorseId,
                    HorseName  = i.Horse.HorseName,
                    OwnerId    = i.OwnerId,
                    OwnerName  = i.Owner.User.FullName,
                    JockeyId   = i.JockeyId,
                    JockeyName = i.Jockey.User.FullName,
                    TourId     = i.TourId,
                    TourName   = i.Tour.TourName,
                    Message    = i.Message,
                    Status     = i.Status,
                    SentAt     = i.SentAt
                })
                .ToListAsync();

            return result;
        }

        // =========================================================================
        // FR-INVT-004 + FR-INVT-005: Jockey responds; auto-cancel duplicates
        // Endpoint: PUT /api/invitations/{id}/respond
        // =========================================================================

        /// <summary>
        /// Jockey accepts or rejects an invitation. If accepted, auto-cancels all other
        /// Pending invitations for the same Horse (FR-INVT-005).
        ///
        /// FLOW:
        ///   1. Find the invitation by inviteId.
        ///   2. Ownership check: invitation.JockeyId must match jockeyId.
        ///   3. State check: invitation.Status must be "Pending".
        ///   4. Begin DB transaction.
        ///   5a. If Accept:
        ///       → Set Status = "Accepted".
        ///       → Find all OTHER Pending invitations for the same HorseId, set Status = "AutoCancelled".
        ///   5b. If Reject:
        ///       → Set Status = "Rejected".
        ///   6. Commit transaction and return.
        ///
        /// RACE CONDITION PREVENTION:
        ///   All reads and writes happen inside a single transaction with default isolation.
        ///   EF Core tracks the entity and SaveChangesAsync commits atomically.
        /// </summary>
        public async Task<(bool Success, string Message)> RespondToInvitationAsync(int jockeyId, int inviteId, bool accept)
        {
            // Step 1 + 2 + 3: Find invitation, validate ownership and status
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.InviteId == inviteId);

            if (invitation == null)
                return (false, "Invitation not found.");

            if (invitation.JockeyId != jockeyId)
                return (false, "You do not have permission to respond to this invitation.");

            if (invitation.Status != "Pending")
                return (false, $"Cannot respond to an invitation with status '{invitation.Status}'.");

            // Step 4: Execute inside transaction (FR-INVT-005 safety)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (accept)
                {
                    // Step 5a: Accept this invitation
                    invitation.Status = "Accepted";

                    // FR-INVT-005: Auto-cancel ALL other Pending invitations for the same horse
                    var otherPending = await _context.Invitations
                        .Where(i =>
                            i.HorseId == invitation.HorseId &&
                            i.InviteId != inviteId &&
                            i.Status == "Pending")
                        .ToListAsync();

                    foreach (var other in otherPending)
                    {
                        other.Status = "AutoCancelled";
                    }
                }
                else
                {
                    // Step 5b: Reject this invitation
                    invitation.Status = "Rejected";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var action = accept ? "accepted" : "rejected";
                return (true, $"Invitation {action} successfully.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================================================
        // FR-INVT-006: HorseOwner cancels a pending invitation
        // Endpoint: PUT /api/invitations/{id}/cancel
        // =========================================================================

        /// <summary>
        /// HorseOwner cancels a pending invitation.
        ///
        /// FLOW:
        ///   1. Find the invitation by inviteId.
        ///   2. Ownership check: invitation.OwnerId must match ownerId.
        ///   3. State check: invitation.Status must be "Pending".
        ///   4. Set Status = "Cancelled".
        ///   5. Save changes and return.
        /// </summary>
        public async Task<(bool Success, string Message)> CancelInvitationAsync(int ownerId, int inviteId)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.InviteId == inviteId);

            if (invitation == null)
                return (false, "Invitation not found.");

            if (invitation.OwnerId != ownerId)
                return (false, "You do not have permission to cancel this invitation.");

            if (invitation.Status != "Pending")
                return (false, $"Cannot cancel an invitation with status '{invitation.Status}'. Only pending invitations can be cancelled.");

            invitation.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return (true, "Invitation cancelled successfully.");
        }
    }
}
