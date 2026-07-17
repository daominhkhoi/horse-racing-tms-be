using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Modules.Races.DTOs
{
    public class SubmitRaceResultDto
    {
        public List<ParticipantResultDto> ParticipantResults { get; set; } = new List<ParticipantResultDto>();
    }

    public class ParticipantResultDto
    {
        public int ParticipantId { get; set; }
        public int? RankPosition { get; set; }
        public TimeOnly? FinishTime { get; set; }
        public string? ResultStatus { get; set; } // Finished, DNF, DSQ
    }

    public class ResultViewDto
    {
        public int ResultId { get; set; }
        public int ParticipantId { get; set; }
        public int HorseId { get; set; }
        public string? HorseName { get; set; }
        public string? HorseAvatar { get; set; }
        public int? JockeyId { get; set; }
        public string? JockeyName { get; set; }
        public string? JockeyAvatar { get; set; }
        public int? LaneNumber { get; set; }
        
        public TimeOnly? FinishTime { get; set; }
        public int? RankPosition { get; set; }
        public string? ResultStatus { get; set; }
        public decimal? RewardMoney { get; set; }
    }
}
