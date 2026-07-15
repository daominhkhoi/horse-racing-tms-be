using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HorseRacingTournamentManagementSystem_0.Modules.Tournaments.DTOs
{
    public class CreateTournamentDto
    {
        [Required(ErrorMessage = "Tournament Name is required.")]
        [StringLength(150, ErrorMessage = "Tournament Name cannot exceed 150 characters.")]
        public string TourName { get; set; } = null!;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "Prize Pool is required.")]
        [Range(0, 1000000000, ErrorMessage = "Prize Pool must be a positive number.")]
        public decimal? PrizePool { get; set; }

        public List<CreateRaceDto> Races { get; set; } = new List<CreateRaceDto>();
    }

    public class CreateRaceDto
    {
        [Required(ErrorMessage = "Race Name is required.")]
        [StringLength(150, ErrorMessage = "Race Name cannot exceed 150 characters.")]
        public string? RaceName { get; set; }

        [Required(ErrorMessage = "Race DateTime is required.")]
        public DateTime? RaceDateTime { get; set; }

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0, 100000, ErrorMessage = "Distance must be a positive number.")]
        public double? Distance { get; set; }

        [Required(ErrorMessage = "Reward Ratio is required.")]
        [Range(1.1, 100, ErrorMessage = "Reward Ratio must be between 1.1 and 100.")]
        public double? RewardRatio { get; set; }

        public List<int> RefereeIds { get; set; } = new List<int>();

        public List<CreateRaceParticipantDto> Participants { get; set; } = new List<CreateRaceParticipantDto>();
    }

    public class CreateRaceParticipantDto
    {
        [Required(ErrorMessage = "Lane Number is required.")]
        [Range(1, 100, ErrorMessage = "Lane Number must be between 1 and 100.")]
        public int? LaneNumber { get; set; }

        [Required(ErrorMessage = "Horse is required.")]
        public int HorseId { get; set; }

        [Required(ErrorMessage = "Jockey is required.")]
        public int JockeyId { get; set; }
    }
}
