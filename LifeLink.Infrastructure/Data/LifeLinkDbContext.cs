using System;
using System.Collections.Generic;
using LifeLink.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LifeLink.Infrastructure.Data;

public partial class LifeLinkDbContext : DbContext
{
    public LifeLinkDbContext(DbContextOptions<LifeLinkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AiinteractionLog> AiinteractionLogs { get; set; }

    public virtual DbSet<BloodRequest> BloodRequests { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<DonorProfile> DonorProfiles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<RequestResponse> RequestResponses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AiinteractionLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AIIntera__3214EC07FA03E2B5");

            entity.ToTable("AIInteractionLogs");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_AIInteractionLogs_UserId_CreatedAt").IsDescending(false, true);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_AIInteractionLogs_CreatedAt");
            entity.Property(e => e.FeatureName).HasMaxLength(100);
            entity.Property(e => e.ModelName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.AiinteractionLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AIInteractionLogs_Users");
        });

        modelBuilder.Entity<BloodRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BloodReq__3214EC078B8923C2");

            entity.HasIndex(e => new { e.Status, e.CityId }, "IX_BloodRequests_Status_City");

            entity.Property(e => e.BloodGroup)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_BloodRequests_CreatedAt");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.HospitalAddress).HasMaxLength(500);
            entity.Property(e => e.HospitalName).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Open", "DF_BloodRequests_Status");
            entity.Property(e => e.Urgency).HasMaxLength(20);

            entity.HasOne(d => d.City).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodRequests_Cities");

            entity.HasOne(d => d.Requester).WithMany(p => p.BloodRequests)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BloodRequests_Users");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cities__3214EC07D8AC10B4");

            entity.HasIndex(e => new { e.Name, e.State }, "UQ_Cities_Name_State").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
        });

        modelBuilder.Entity<DonorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DonorPro__3214EC07A80FF84A");

            entity.HasIndex(e => new { e.BloodGroup, e.CityId, e.IsAvailable, e.IsVerified }, "IX_DonorProfiles_BloodGroup_City_Availability");

            entity.HasIndex(e => e.Location, "IX_DonorProfiles_Location");

            entity.HasIndex(e => e.UserId, "UQ_DonorProfiles_UserId").IsUnique();

            entity.Property(e => e.BloodGroup)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_DonorProfiles_CreatedAt");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true, "DF_DonorProfiles_IsAvailable");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

            entity.HasOne(d => d.City).WithMany(p => p.DonorProfiles)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonorProfiles_Cities");

            entity.HasOne(d => d.User).WithOne(p => p.DonorProfile)
                .HasForeignKey<DonorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DonorProfiles_Users");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3214EC07E133CCDE");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_Notifications_UserId_IsRead");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Notifications_CreatedAt");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.BloodRequestId)
                .HasConstraintName("FK_Notifications_BloodRequests");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<RequestResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RequestR__3214EC07AD7D7F6D");

            entity.HasIndex(e => e.BloodRequestId, "IX_RequestResponses_BloodRequestId");

            entity.HasIndex(e => new { e.BloodRequestId, e.DonorId }, "UQ_RequestResponses_Request_Donor").IsUnique();

            entity.Property(e => e.RespondedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_RequestResponses_RespondedAt");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending", "DF_RequestResponses_Status");

            entity.HasOne(d => d.BloodRequest).WithMany(p => p.RequestResponses)
                .HasForeignKey(d => d.BloodRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestResponses_BloodRequests");

            entity.HasOne(d => d.Donor).WithMany(p => p.RequestResponses)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RequestResponses_DonorProfiles");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07587C08B6");

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Users_CreatedAt");
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Users_IsActive");
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
