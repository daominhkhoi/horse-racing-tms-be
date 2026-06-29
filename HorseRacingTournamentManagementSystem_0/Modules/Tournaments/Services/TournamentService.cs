using System;
using System.Linq;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly HorseRacingDbContext _context;

        public TournamentService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<Tournament> CreateTournamentAsync(CreateTournamentDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tournament = new Tournament
                {
                    TourName = dto.TourName,
                    Location = dto.Location,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    PrizePool = dto.PrizePool,
                    Status = "Upcoming"
                };

                _context.Tournaments.Add(tournament);
                await _context.SaveChangesAsync();

                int roundNumber = 1;
                foreach (var raceDto in dto.Races)
                {
                    var race = new Race
                    {
                        TourId = tournament.TourId,
                        RaceName = raceDto.RaceName,
                        Round = roundNumber++,
                        RaceDateTime = raceDto.RaceDateTime,
                        Distance = raceDto.Distance,
                        Status = "Upcoming"
                    };

                    _context.Races.Add(race);
                    await _context.SaveChangesAsync();

                    foreach (var participantDto in raceDto.Participants)
                    {
                        var participant = new RaceParticipant
                        {
                            RaceId = race.RaceId,
                            HorseId = participantDto.HorseId,
                            JockeyId = participantDto.JockeyId,
                            LaneNumber = participantDto.LaneNumber,
                            ParticipationStatus = "Pending"
                        };

                        _context.RaceParticipants.Add(participant);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return tournament;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
