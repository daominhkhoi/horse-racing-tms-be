using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class HorseVerification
{
    public int VerifyId { get; set; }

    public int HorseId { get; set; }

    public int? VerifiedBy { get; set; }

    public DateTime? VerifyDate { get; set; }

    public string? InspectionUrl { get; set; }

    public string? HealthCertUrl { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }

    public virtual Horse Horse { get; set; } = null!;

    public virtual User? VerifiedByNavigation { get; set; }
}
