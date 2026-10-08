using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RmsResultViewer.Data.Entities;

namespace RmsResultViewer.Data;

public partial class RmsResultDbContext : DbContext
{
    public RmsResultDbContext(DbContextOptions<RmsResultDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbRmsResult> TbRmsResults { get; set; }

    public virtual DbSet<TbRmsResultParameter> TbRmsResultParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbRmsResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TB_RMS_R__A61560380456AC35");

            entity.ToTable("TB_RMS_RESULT");

            entity.HasIndex(e => new { e.EquipId, e.EventTime }, "IX_RMS_RESULT_EQUIP_TIME").IsDescending(false, true);

            entity.HasIndex(e => e.EventTime, "IX_RMS_RESULT_EVENT_TIME").IsDescending();

            entity.Property(e => e.ResultId).HasColumnName("RESULT_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EquipId)
                .HasMaxLength(50)
                .HasColumnName("EQUIP_ID");
            entity.Property(e => e.EventTime)
                .HasPrecision(3)
                .HasColumnName("EVENT_TIME");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
        });

        modelBuilder.Entity<TbRmsResultParameter>(entity =>
        {
            entity.HasKey(e => new { e.ResultId, e.ParameterId }).HasName("PK_RESULT_PARAMETER");

            entity.ToTable("TB_RMS_RESULT_PARAMETER");

            entity.Property(e => e.ResultId).HasColumnName("RESULT_ID");
            entity.Property(e => e.ParameterId)
                .HasMaxLength(50)
                .HasColumnName("PARAMETER_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.ParameterValue)
                .HasMaxLength(500)
                .HasColumnName("PARAMETER_VALUE");

            entity.HasOne(d => d.Result).WithMany(p => p.TbRmsResultParameters)
                .HasForeignKey(d => d.ResultId)
                .HasConstraintName("FK_RESULT_PARAMETER");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
