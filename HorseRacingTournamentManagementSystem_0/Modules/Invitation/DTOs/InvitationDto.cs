using System;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitation.DTOs
{
    /// <summary>
    /// Response DTO for Invitation – flattened view used by both HorseOwner and Jockey.
    ///
    /// Avoids circular references from navigation properties.
    /// </summary>
    public class InvitationDto
    {
        public int InviteId { get; set; }

        // ── Horse ──────────────────────────────────────────────────────────────
        public int HorseId { get; set; }
        public string HorseName { get; set; } = string.Empty;

        // ── Owner ──────────────────────────────────────────────────────────────
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;

        // ── Jockey ────────────────────────────────────────────────────────────
        public int JockeyId { get; set; }
        public string JockeyName { get; set; } = string.Empty;

        // ── Tournament ────────────────────────────────────────────────────────
        public int TourId { get; set; }
        public string TourName { get; set; } = string.Empty;

        // ── Invitation ────────────────────────────────────────────────────────
        public string? Message { get; set; }

        /// <summary>
        /// Status values: Pending | Accepted | Rejected | Cancelled | AutoCancelled
        /// </summary>
        public string? Status { get; set; }

        public DateTime? SentAt { get; set; }
    }
}
