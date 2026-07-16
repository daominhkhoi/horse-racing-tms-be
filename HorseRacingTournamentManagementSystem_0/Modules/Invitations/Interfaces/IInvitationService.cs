using HorseRacingTournamentManagementSystem_0.Modules.Invitations.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HorseRacingTournamentManagementSystem_0.Modules.Invitations.Interfaces
{
    public interface IInvitationService
    {
        Task<InvitationResponse> SendInvitationAsync(int ownerId, SendInvitationRequest request);
        Task<bool> CancelInvitationAsync(int ownerId, int inviteId);
        Task<IEnumerable<InvitationResponse>> GetMyInvitationsAsync(int jockeyId, string? status = null);
        Task<IEnumerable<InvitationResponse>> GetSentInvitationsAsync(int ownerId, string? status = null);
        Task<bool> RespondToInvitationAsync(int jockeyId, int inviteId, bool isAccepted);
    }
}
