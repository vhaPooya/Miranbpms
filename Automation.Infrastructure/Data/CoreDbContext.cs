using Automation.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Data;

/// <summary>
/// Bounded context for static/reference data: Secretariat, connection strings, web services,
/// action types, priorities, statuses, confidentiality levels, themes.
/// Used for dropdowns and settings. Same database as Identity and Automation; no EF for dynamic data (Form/Workflow execution).
/// </summary>
public class CoreDbContext : DbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }

    /// <summary>سازمان (برای رابطه با Secretariat در همین context؛ همان جدول Identity است)</summary>
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<Secretariat> Secretariats { get; set; } = null!;
    public DbSet<ConnectionString> ConnectionStrings { get; set; } = null!;
    public DbSet<WebService> WebServices { get; set; } = null!;
    public DbSet<ActionType> ActionTypes { get; set; } = null!;
    public DbSet<Priority> Priorities { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;
    public DbSet<ConfidentialityLevel> ConfidentialityLevels { get; set; } = null!;
    public DbSet<Theme> Themes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureOrganization(modelBuilder);
        ConfigureSecretariat(modelBuilder);
        ConfigureConnectionString(modelBuilder);
        ConfigureWebService(modelBuilder);
        ConfigureActionType(modelBuilder);
        ConfigurePriority(modelBuilder);
        ConfigureStatus(modelBuilder);
        ConfigureConfidentialityLevel(modelBuilder);
        ConfigureTheme(modelBuilder);

        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Secretariat>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConnectionString>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WebService>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ActionType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Priority>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Status>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConfidentialityLevel>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Theme>().HasQueryFilter(e => !e.IsDeleted);
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrganizationCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OrganizationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.OrganizationCode).IsUnique();
        });
    }

    private static void ConfigureSecretariat(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Secretariat>(entity =>
        {
            entity.ToTable("Secretariats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SecretariatCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SecretariatName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EmailAddress).HasMaxLength(255);
            entity.Property(e => e.FaxNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RegistryType).HasMaxLength(50);
            entity.Property(e => e.IncomingNumberFormat).HasMaxLength(200);
            entity.Property(e => e.OutgoingNumberFormat).HasMaxLength(200);
            entity.Property(e => e.IncomingPrefix).HasMaxLength(50);
            entity.Property(e => e.OutgoingPrefix).HasMaxLength(50);
            entity.HasIndex(e => e.SecretariatCode).IsUnique();
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Secretariats)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureConnectionString(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectionString>(entity =>
        {
            entity.ToTable("ConnectionStrings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ConnectionStringValue).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.DatabaseType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private static void ConfigureWebService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebService>(entity =>
        {
            entity.ToTable("WebServices");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ServiceUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ServiceType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HttpMethod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Headers).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AuthType).HasMaxLength(50);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.ServiceName).IsUnique();
        });
    }

    private static void ConfigureActionType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionType>(entity =>
        {
            entity.ToTable("ActionTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActionNameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ActionNameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.ActionCode).IsUnique();
        });
    }

    private static void ConfigurePriority(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Priority>(entity =>
        {
            entity.ToTable("Priorities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PriorityCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PriorityName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.PriorityCode).IsUnique();
        });
    }

    private static void ConfigureStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Statuses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatusCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StatusName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.StatusCode).IsUnique();
        });
    }

    private static void ConfigureConfidentialityLevel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfidentialityLevel>(entity =>
        {
            entity.ToTable("ConfidentialityLevels");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LevelCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LevelName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.LevelCode).IsUnique();
        });
    }

    private static void ConfigureTheme(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.ToTable("Themes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ThemeName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PrimaryColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SecondaryColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BackgroundColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TextColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FontFamily).HasMaxLength(200);
            entity.Property(e => e.CustomCss).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => e.ThemeName).IsUnique();
        });
    }
}
