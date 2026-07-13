using System;
using System.Linq;
using System.Threading.Tasks;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using HorseRacingTournamentManagementSystem_0.Modules.Common.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs;
using HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.Services
{
    public class TournamentService : ITournamentService
    {
        private readonly HorseRacingDbContext _context;

        public TournamentService(HorseRacingDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<TournamentDto>> GetAllTournamentsAsync(int page, int pageSize, string? searchTerm, bool includeHidden = false)
        {
            var query = _context.Tournaments.AsQueryable();

            if (!includeHidden)
            {
                query = query.Where(t => !t.IsHidden);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => 
                    t.TourName.Contains(searchTerm) || 
                    (t.Location != null && t.Location.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(t => t.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TournamentDto
                {
                    TourId = t.TourId,
                    TourName = t.TourName,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Location = t.Location,
                    PrizePool = t.PrizePool,
                    Status = t.Status,
                    IsHidden = t.IsHidden
                })
                .ToListAsync();

            return new PagedResult<TournamentDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<TournamentDetailDto> GetTournamentByIdAsync(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Races)
                    .ThenInclude(r => r.RaceParticipants)
                        .ThenInclude(rp => rp.Horse)
                .Include(t => t.Races)
                    .ThenInclude(r => r.RaceParticipants)
                        .ThenInclude(rp => rp.Jockey)
                            .ThenInclude(j => j.User)
                .Include(t => t.Races)
                    .ThenInclude(r => r.RefereeAssignments)
                        .ThenInclude(ra => ra.Referee)
                            .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.TourId == id);

            if (tournament == null)
            {
                throw new Exception("Tournament not found");
            }

            var detailDto = new TournamentDetailDto
            {
                TourId = tournament.TourId,
                TourName = tournament.TourName,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                Location = tournament.Location,
                PrizePool = tournament.PrizePool,
                Status = tournament.Status,
                IsHidden = tournament.IsHidden,
                Races = tournament.Races.Select(r => new RaceDto
                {
                    RaceId = r.RaceId,
                    RaceName = r.RaceName,
                    Round = r.Round,
                    RaceDateTime = r.RaceDateTime,
                    Distance = r.Distance,
                    Status = r.Status,
                    Participants = r.RaceParticipants.Select(p => new RaceParticipantDto
                    {
                        ParticipantId = p.ParticipantId,
                        HorseId = p.HorseId,
                        HorseName = p.Horse?.HorseName,
                        HorseAvatar = p.Horse?.ImageUrl,
                        JockeyId = p.JockeyId,
                        JockeyName = p.Jockey?.User?.FullName,
                        JockeyAvatar = p.Jockey?.Avatar,
                        LaneNumber = p.LaneNumber,
                        ParticipationStatus = p.ParticipationStatus
                    }).ToList(),
                    Referees = r.RefereeAssignments.Select(ra => new RefereeAssignmentDto
                    {
                        AssignId = ra.AssignId,
                        RefereeId = ra.RefereeId,
                        RefereeName = ra.Referee?.User?.FullName,
                        RefereeAvatar = ra.Referee?.Avatar
                    }).ToList()
                }).ToList()
            };

            return detailDto;
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

                    if (raceDto.RefereeIds != null && raceDto.RefereeIds.Any())
                    {
                        foreach (var refereeId in raceDto.RefereeIds)
                        {
                            var refereeAssignment = new RefereeAssignment
                            {
                                RaceId = race.RaceId,
                                RefereeId = refereeId,
                                AssignedAt = DateTime.UtcNow
                            };
                            _context.RefereeAssignments.Add(refereeAssignment);
                        }
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

        public async Task<Tournament> UpdateTournamentAsync(int id, CreateTournamentDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var tournament = await _context.Tournaments
                    .Include(t => t.Races)
                        .ThenInclude(r => r.RaceParticipants)
                    .Include(t => t.Races)
                        .ThenInclude(r => r.RefereeAssignments)
                    .FirstOrDefaultAsync(t => t.TourId == id);

                if (tournament == null)
                {
                    throw new Exception("Tournament not found");
                }

                if (tournament.Status != "Upcoming")
                {
                    throw new Exception("Can only edit tournaments that are 'Upcoming'");
                }

                // Update basic fields
                tournament.TourName = dto.TourName;
                tournament.Location = dto.Location;
                tournament.StartDate = dto.StartDate;
                tournament.EndDate = dto.EndDate;
                tournament.PrizePool = dto.PrizePool;

                // Remove old races (cascade delete will handle participants and referees if configured, otherwise manual delete)
                // EF Core usually handles deleting dependents if configured, but to be safe, we explicitly remove them:
                foreach (var oldRace in tournament.Races.ToList())
                {
                    _context.RaceParticipants.RemoveRange(oldRace.RaceParticipants);
                    _context.RefereeAssignments.RemoveRange(oldRace.RefereeAssignments);
                    _context.Races.Remove(oldRace);
                }
                await _context.SaveChangesAsync(); // save deletions first

                // Add new races
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

                    if (raceDto.RefereeIds != null && raceDto.RefereeIds.Any())
                    {
                        foreach (var refereeId in raceDto.RefereeIds)
                        {
                            var refereeAssignment = new RefereeAssignment
                            {
                                RaceId = race.RaceId,
                                RefereeId = refereeId,
                                AssignedAt = DateTime.UtcNow
                            };
                            _context.RefereeAssignments.Add(refereeAssignment);
                        }
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

        public async Task<bool> CancelTournamentAsync(int id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Races)
                .FirstOrDefaultAsync(t => t.TourId == id);

            if (tournament == null)
            {
                throw new Exception("Tournament not found");
            }

            if (tournament.Status != "Upcoming")
            {
                throw new Exception("Can only cancel tournaments that are 'Upcoming'");
            }

            tournament.Status = "Cancelled";
            foreach (var race in tournament.Races)
            {
                race.Status = "Cancelled";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleTournamentHiddenStatusAsync(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
                return false;

            tournament.IsHidden = !tournament.IsHidden;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
