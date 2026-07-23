using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;
using HorseRacingTournamentManagementSystem_0.Entities;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDbContext<HorseRacingDbContext>(options =>
    options.UseSqlServer("Server=.;Database=HorseRacingDB;User Id=sa;Password=12345;TrustServerCertificate=True;"));

var serviceProvider = services.BuildServiceProvider();
var _context = serviceProvider.GetRequiredService<HorseRacingDbContext>();

var tourId = 11;
var tournament = await _context.Tournaments
    .Include(t => t.Races)
    .FirstOrDefaultAsync(t => t.TourId == tourId);

if (tournament == null) { Console.WriteLine("Tournament null"); return; }
if (!tournament.Races.Any()) { Console.WriteLine("No races"); return; }

Console.WriteLine("Tournament Status Before: " + tournament.Status);
foreach(var r in tournament.Races) {
    Console.WriteLine("Race " + r.RaceId + " Status: '" + r.Status + "'");
}

bool allFinished = tournament.Races.All(r => r.Status == "Completed" || r.Status == "Awarded");
bool anyActive = tournament.Races.Any(r => r.Status == "Started" || r.Status == "Live" || r.Status == "Ongoing" || r.Status == "Completed" || r.Status == "Awarded");

Console.WriteLine("allFinished: " + allFinished);
Console.WriteLine("anyActive: " + anyActive);

if (allFinished)
{
    tournament.Status = "Completed";
}
else if (anyActive)
{
    tournament.Status = "Live";
}
else
{
    tournament.Status = "Upcoming";
}

Console.WriteLine("Tournament Status After: " + tournament.Status);
