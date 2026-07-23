using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Modules.Predictions.DTOs;

public class AiPredictionDto
{
    public string Horse { get; set; } = null!;
    public string Race { get; set; } = null!;
    public int Confidence { get; set; }
    public string Odds { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Breed { get; set; } = null!;
    public string Jockey { get; set; } = null!;
    public string Track { get; set; } = null!;
    public List<string> Form { get; set; } = new();
    public string RaceDate { get; set; } = null!;
    public string Gradient { get; set; } = "from-emerald-500 to-green-600";
    public string AvatarGrad { get; set; } = "from-sky-400 to-blue-600";
    public string? ImageUrl { get; set; }
}
