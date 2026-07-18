using HorseRacingTournamentManagementSystem_0.Entities;
using Microsoft.EntityFrameworkCore;

namespace HorseRacingTournamentManagementSystem_0.Database;

public partial class HorseRacingDbContext : DbContext
{
    public HorseRacingDbContext()
    {
    }

    public HorseRacingDbContext(DbContextOptions<HorseRacingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminProfile> AdminProfiles { get; set; }

    public virtual DbSet<Horse> Horses { get; set; }

    public virtual DbSet<HorseVerification> HorseVerifications { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<JockeyProfile> JockeyProfiles { get; set; }

    public virtual DbSet<Leaderboard> Leaderboards { get; set; }

    public virtual DbSet<OwnerProfile> OwnerProfiles { get; set; }

    public virtual DbSet<Prediction> Predictions { get; set; }

    public virtual DbSet<Race> Races { get; set; }

    public virtual DbSet<RaceComment> RaceComments { get; set; }

    public virtual DbSet<RaceParticipant> RaceParticipants { get; set; }

    public virtual DbSet<RefereeAssignment> RefereeAssignments { get; set; }

    public virtual DbSet<RefereeProfile> RefereeProfiles { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<RewardTransaction> RewardTransactions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SpectatorProfile> SpectatorProfiles { get; set; }

    public virtual DbSet<TopupTransaction> TopupTransactions { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Violation> Violations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }

    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();
        var strConn = config["ConnectionStrings:DefaultConnection"];

        return strConn;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Admin_Pr__1788CCAC309EE008");

            entity.ToTable("Admin_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.AdminProfile)
                .HasForeignKey<AdminProfile>(d => d.UserId)
                .HasConstraintName("FK__Admin_Pro__UserI__3F466844");
        });

        modelBuilder.Entity<Horse>(entity =>
        {
            entity.HasKey(e => e.HorseId).HasName("PK__Horses__418B5D482F4469F8");

            entity.Property(e => e.HorseId).HasColumnName("HorseID");
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.HealthStatus).HasMaxLength(50);
            entity.Property(e => e.HorseName).HasMaxLength(100);
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");

            entity.HasOne(d => d.Owner).WithMany(p => p.Horses)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Horses__OwnerID__5070F446");
        });

        modelBuilder.Entity<HorseVerification>(entity =>
        {
            entity.HasKey(e => e.VerifyId).HasName("PK__Horse_Ve__0A2710A9F454A679");

            entity.ToTable("Horse_Verifications");

            entity.Property(e => e.VerifyId).HasColumnName("VerifyID");
            entity.Property(e => e.HealthCertUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("HealthCert_URL");
            entity.Property(e => e.HorseId).HasColumnName("HorseID");
            entity.Property(e => e.InspectionUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("InspectionURL");
            entity.Property(e => e.Result).HasMaxLength(50);
            entity.Property(e => e.VerifyDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Horse).WithMany(p => p.HorseVerifications)
                .HasForeignKey(d => d.HorseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Horse_Ver__Horse__5441852A");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.HorseVerifications)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("FK__Horse_Ver__Verif__5535A963");
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.InviteId).HasName("PK__Invitati__AFACE80D6AF21674");

            entity.Property(e => e.InviteId).HasColumnName("InviteID");
            entity.Property(e => e.HorseId).HasColumnName("HorseID");
            entity.Property(e => e.JockeyId).HasColumnName("JockeyID");
            entity.Property(e => e.OwnerId).HasColumnName("OwnerID");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Horse).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.HorseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitatio__Horse__01142BA1");

            entity.HasOne(d => d.Jockey).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.JockeyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitatio__Jocke__00200768");

            entity.HasOne(d => d.Owner).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitatio__Owner__7F2BE32F");

            entity.HasOne(d => d.Tour).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invitatio__TourI__02084FDA");
        });

        modelBuilder.Entity<JockeyProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Jockey_P__1788CCAC22021E48");

            entity.ToTable("Jockey_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.JockeyProfile)
                .HasForeignKey<JockeyProfile>(d => d.UserId)
                .HasConstraintName("FK__Jockey_Pr__UserI__44FF419A");
        });

        modelBuilder.Entity<Leaderboard>(entity =>
        {
            entity.HasKey(e => e.BoardId).HasName("PK__Leaderbo__F9646BD243BADE23");

            entity.Property(e => e.BoardId).HasColumnName("BoardID");
            entity.Property(e => e.HorseId).HasColumnName("HorseID");
            entity.Property(e => e.JockeyId).HasColumnName("JockeyID");
            entity.Property(e => e.TotalPoints).HasDefaultValue(0.0);
            entity.Property(e => e.TotalWins).HasDefaultValue(0);
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Horse).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.HorseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Leaderboa__Horse__7A672E12");

            entity.HasOne(d => d.Jockey).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.JockeyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Leaderboa__Jocke__7B5B524B");

            entity.HasOne(d => d.Tour).WithMany(p => p.Leaderboards)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Leaderboa__TourI__797309D9");
        });

        modelBuilder.Entity<OwnerProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Owner_Pr__1788CCACD9601BE7");

            entity.ToTable("Owner_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.OwnerProfile)
                .HasForeignKey<OwnerProfile>(d => d.UserId)
                .HasConstraintName("FK__Owner_Pro__UserI__4222D4EF");
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.HasKey(e => e.PredictionId).HasName("PK__Predicti__BAE4C140DDF24377");

            entity.Property(e => e.PredictionId).HasColumnName("PredictionID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.RaceId).HasColumnName("RaceID");
            entity.Property(e => e.SpectatorId).HasColumnName("SpectatorID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Participant).WithMany(p => p.Predictions)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Predictio__Parti__6FE99F9F");

            entity.HasOne(d => d.Race).WithMany(p => p.Predictions)
                .HasForeignKey(d => d.RaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Predictio__RaceI__6E01572D");

            entity.HasOne(d => d.Spectator).WithMany(p => p.Predictions)
                .HasForeignKey(d => d.SpectatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Predictio__Spect__6EF57B66");
        });

        modelBuilder.Entity<Race>(entity =>
        {
            entity.HasKey(e => e.RaceId).HasName("PK__Races__05FBD6D4821B4B8F");

            entity.Property(e => e.RaceId).HasColumnName("RaceID");
            entity.Property(e => e.RaceDateTime).HasColumnType("datetime");
            entity.Property(e => e.RaceName).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TourId).HasColumnName("TourID");

            entity.HasOne(d => d.Tour).WithMany(p => p.Races)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Races__TourId__5165187F");
        });

        modelBuilder.Entity<RaceComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_RaceComments");

            entity.ToTable("RaceComments");

            entity.Property(e => e.Content).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Race).WithMany(p => p.RaceComments)
                .HasForeignKey(d => d.RaceId)
                .HasConstraintName("FK_RaceComments_Races");

            entity.HasOne(d => d.User).WithMany(p => p.RaceComments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_RaceComments_Users");
        });

        modelBuilder.Entity<RaceParticipant>(entity =>
        {
            entity.HasKey(e => e.ParticipantId).HasName("PK__Race_Par__7227997EBDCB2F76");

            entity.ToTable("Race_Participants");

            entity.HasIndex(e => new { e.RaceId, e.HorseId, e.JockeyId }, "UQ_Race_Horse_Jockey").IsUnique();

            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.CheckInStatus).HasMaxLength(50);
            entity.Property(e => e.HorseId).HasColumnName("HorseID");
            entity.Property(e => e.JockeyId).HasColumnName("JockeyID");
            entity.Property(e => e.ParticipationStatus).HasMaxLength(50);
            entity.Property(e => e.RaceId).HasColumnName("RaceID");

            entity.HasOne(d => d.Horse).WithMany(p => p.RaceParticipants)
                .HasForeignKey(d => d.HorseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Race_Part__Horse__619B8048");

            entity.HasOne(d => d.Jockey).WithMany(p => p.RaceParticipants)
                .HasForeignKey(d => d.JockeyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Race_Part__Jocke__628FA481");

            entity.HasOne(d => d.Race).WithMany(p => p.RaceParticipants)
                .HasForeignKey(d => d.RaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Race_Part__RaceI__60A75C0F");
        });

        modelBuilder.Entity<RefereeAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignId).HasName("PK__Referee___9FFF4C4FB8D4112D");

            entity.ToTable("Referee_Assignments");

            entity.Property(e => e.AssignId).HasColumnName("AssignID");
            entity.Property(e => e.AssignedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RaceId).HasColumnName("RaceID");
            entity.Property(e => e.RefereeId).HasColumnName("RefereeID");

            entity.HasOne(d => d.Race).WithMany(p => p.RefereeAssignments)
                .HasForeignKey(d => d.RaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Referee_A__RaceI__5BE2A6F2");

            entity.HasOne(d => d.Referee).WithMany(p => p.RefereeAssignments)
                .HasForeignKey(d => d.RefereeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Referee_A__Refer__5CD6CB2B");
        });

        modelBuilder.Entity<RefereeProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Referee___1788CCAC65930114");

            entity.ToTable("Referee_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.RefereeProfile)
                .HasForeignKey<RefereeProfile>(d => d.UserId)
                .HasConstraintName("FK__Referee_P__UserI__47DBAE45");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC0751609FE4");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Results__97690228615F78EC");

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.RaceId).HasColumnName("RaceID");
            entity.Property(e => e.ResultStatus).HasMaxLength(50);
            entity.Property(e => e.RewardMoney).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Participant).WithMany(p => p.Results)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Results__Partici__66603565");

            entity.HasOne(d => d.Race).WithMany(p => p.Results)
                .HasForeignKey(d => d.RaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Results__RaceID__656C112C");
        });

        modelBuilder.Entity<RewardTransaction>(entity =>
        {
            entity.HasKey(e => e.TranId).HasName("PK__Reward_T__F70896298560BA61");

            entity.ToTable("Reward_Transactions");

            entity.Property(e => e.TranId).HasColumnName("TranID");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PredictionId).HasColumnName("PredictionID");
            entity.Property(e => e.SpectatorId).HasColumnName("SpectatorID");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Prediction).WithMany(p => p.RewardTransactions)
                .HasForeignKey(d => d.PredictionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reward_Tr__Predi__74AE54BC");

            entity.HasOne(d => d.Spectator).WithMany(p => p.RewardTransactions)
                .HasForeignKey(d => d.SpectatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reward_Tr__Spect__73BA3083");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3A12AD03F9");

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<SpectatorProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Spectato__1788CCAC6443E64B");

            entity.ToTable("Spectator_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalPoints).HasDefaultValue(0.0);

            entity.HasOne(d => d.User).WithOne(p => p.SpectatorProfile)
                .HasForeignKey<SpectatorProfile>(d => d.UserId)
                .HasConstraintName("FK__Spectator__UserI__4BAC3F29");
        });

        modelBuilder.Entity<TopupTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TopupTransactions");

            entity.ToTable("TopupTransactions");

            entity.Property(e => e.Amount).HasColumnType("float");
            entity.Property(e => e.PointsAdded).HasColumnType("float");
            entity.Property(e => e.VnpTxnRef).HasMaxLength(255);
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");

            entity.HasOne(d => d.Spectator).WithMany(p => p.TopupTransactions)
                .HasForeignKey(d => d.SpectatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopupTransactions_SpectatorProfiles");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.TourId).HasName("PK__Tourname__604CEA10BA81FC81");

            entity.Property(e => e.TourId).HasColumnName("TourID");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.PrizePool).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TourName).HasMaxLength(150);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC00CC205C");

            entity.ToTable(tb => tb.HasTrigger("trg_AfterInsertUser_CreateProfile"));

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534091DAFE2").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.FullName).HasMaxLength(100);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleID__3C69FB99");
        });

        modelBuilder.Entity<Violation>(entity =>
        {
            entity.HasKey(e => e.ViolationId).HasName("PK__Violatio__18B6DC282669EBE9");

            entity.Property(e => e.ViolationId).HasColumnName("ViolationID");
            entity.Property(e => e.ParticipantId).HasColumnName("ParticipantID");
            entity.Property(e => e.Penalty).HasMaxLength(255);
            entity.Property(e => e.RaceId).HasColumnName("RaceID");
            entity.Property(e => e.RefereeId).HasColumnName("RefereeID");
            entity.Property(e => e.ViolationType).HasMaxLength(100);

            entity.HasOne(d => d.Participant).WithMany(p => p.Violations)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Violation__Parti__6A30C649");

            entity.HasOne(d => d.Race).WithMany(p => p.Violations)
                .HasForeignKey(d => d.RaceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Violation__RaceI__693CA210");

            entity.HasOne(d => d.Referee).WithMany(p => p.Violations)
                .HasForeignKey(d => d.RefereeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Violation__Refer__6B24EA82");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
