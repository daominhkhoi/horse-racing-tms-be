using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HorseRacingTournamentManagementSystem_0.Database;

var optionsBuilder = new DbContextOptionsBuilder<HorseRacingDbContext>();
optionsBuilder.UseSqlServer("Server=.;Database=HorseRacingDB;User Id=sa;Password=12345;TrustServerCertificate=True;");
var db = new HorseRacingDbContext(optionsBuilder.Options);

var horses = db.Horses
    .Include(h => h.Owner)
    .ThenInclude(o => o.User)
    .Include(h => h.HorseVerifications)
    .Where(h => h.HorseVerifications.OrderByDescending(v => v.VerifyDate).FirstOrDefault().Result == "Pending" || !h.HorseVerifications.Any())
    .ToList();

Console.WriteLine("Pending Horses count: " + horses.Count);
foreach(var h in horses) {
    Console.WriteLine("- " + h.HorseId + " " + h.HorseName);
}
