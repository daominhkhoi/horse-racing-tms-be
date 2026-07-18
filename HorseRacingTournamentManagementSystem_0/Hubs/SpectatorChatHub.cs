using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HorseRacingTournamentManagementSystem_0.Hubs
{
    public class SpectatorChatHub : Hub
    {
        private readonly HorseRacingDbContext _context;

        public SpectatorChatHub(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task JoinRaceGroup(string raceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, raceId);
        }

        public async Task LeaveRaceGroup(string raceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, raceId);
        }

        public async Task SendMessage(string raceId, int userId, string content)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Save to DB
            var comment = new RaceComment
            {
                RaceId = int.Parse(raceId),
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.RaceComments.Add(comment);
            await _context.SaveChangesAsync();

            // Broadcast to group
            await Clients.Group(raceId).SendAsync("ReceiveMessage", new
            {
                user = user.FullName, // Or UserName
                text = content,
                time = DateTime.UtcNow.AddHours(7).ToString("HH:mm"),
                hot = false
            });
        }
    }
}
