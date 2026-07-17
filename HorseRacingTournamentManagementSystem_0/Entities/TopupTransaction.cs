using System;
using System.Collections.Generic;

namespace HorseRacingTournamentManagementSystem_0.Entities;

public partial class TopupTransaction
{
    public int Id { get; set; }

    public int SpectatorId { get; set; }

    public double Amount { get; set; }

    public double PointsAdded { get; set; }

    public string VnpTxnRef { get; set; } = null!;

    public DateTime? TransactionDate { get; set; }

    public string? Status { get; set; }

    public virtual SpectatorProfile Spectator { get; set; } = null!;
}
