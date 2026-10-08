using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RmsKafkaMiddleware.Data.Entities;

namespace RmsKafkaMiddleware.Data;

public partial class RmsDbContext : DbContext
{
    public RmsDbContext(DbContextOptions<RmsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbEquipment> TbEquipments { get; set; }

    public virtual DbSet<TbRmsRecipe> TbRmsRecipes { get; set; }

    public virtual DbSet<TbRmsRecipeParameter> TbRmsRecipeParameters { get; set; }

    public virtual DbSet<TbRmsRequestLog> TbRmsRequestLogs { get; set; }

    public virtual DbSet<TbRmsResult> TbRmsResults { get; set; }

    public virtual DbSet<TbRmsResultParameter> TbRmsResultParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbEquipment>(entity =>
        {
            entity.HasKey(e => e.EquipId).HasName("PK__TB_EQUIP__8C95A5B39BAA7007");

            entity.ToTable("TB_EQUIPMENT");

            entity.Property(e => e.EquipId)
                .HasMaxLength(50)
                .HasColumnName("EQUIP_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EquipName)
                .HasMaxLength(100)
                .HasColumnName("EQUIP_NAME");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasColumnName("UPDATED_AT");
            entity.Property(e => e.UseYn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("USE_YN");
        });

        modelBuilder.Entity<TbRmsRecipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId).HasName("PK__TB_RMS_R__84399C18AB4F7A6B");

            entity.ToTable("TB_RMS_RECIPE");

            entity.HasIndex(e => new { e.EquipId, e.FactorId, e.FactorValue }, "UQ_RECIPE_CONDITION").IsUnique();

            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EquipId)
                .HasMaxLength(50)
                .HasColumnName("EQUIP_ID");
            entity.Property(e => e.FactorId)
                .HasMaxLength(50)
                .HasColumnName("FACTOR_ID");
            entity.Property(e => e.FactorValue)
                .HasMaxLength(200)
                .HasColumnName("FACTOR_VALUE");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasColumnName("UPDATED_AT");
            entity.Property(e => e.UseYn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("USE_YN");

            entity.HasOne(d => d.Equip).WithMany(p => p.TbRmsRecipes)
                .HasForeignKey(d => d.EquipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RECIPE_EQUIP");
        });

        modelBuilder.Entity<TbRmsRecipeParameter>(entity =>
        {
            entity.HasKey(e => new { e.RecipeId, e.ParameterId }).HasName("PK_RECIPE_PARAMETER");

            entity.ToTable("TB_RMS_RECIPE_PARAMETER");

            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
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
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasColumnName("UPDATED_AT");

            entity.HasOne(d => d.Recipe).WithMany(p => p.TbRmsRecipeParameters)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_PARAMETER_RECIPE");
        });

        modelBuilder.Entity<TbRmsRequestLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__TB_RMS_R__4364C88257E12112");

            entity.ToTable("TB_RMS_REQUEST_LOG");

            entity.HasIndex(e => e.MessageKey, "UX_RMS_REQUEST_LOG_SUCCESS")
                .IsUnique()
                .HasFilter("([RESULT_CODE]=N'SUCCESS')");

            entity.Property(e => e.LogId).HasColumnName("LOG_ID");
            entity.Property(e => e.EquipId)
                .HasMaxLength(50)
                .HasColumnName("EQUIP_ID");
            entity.Property(e => e.KafkaOffset).HasColumnName("KAFKA_OFFSET");
            entity.Property(e => e.KafkaPartition).HasColumnName("KAFKA_PARTITION");
            entity.Property(e => e.KafkaTopic)
                .HasMaxLength(100)
                .HasColumnName("KAFKA_TOPIC");
            entity.Property(e => e.MessageKey)
                .HasMaxLength(64)
                .HasColumnName("MESSAGE_KEY");
            entity.Property(e => e.ReceivedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("RECEIVED_AT");
            entity.Property(e => e.RequestXml).HasColumnName("REQUEST_XML");
            entity.Property(e => e.ResponseXml).HasColumnName("RESPONSE_XML");
            entity.Property(e => e.ResultCode)
                .HasMaxLength(30)
                .HasColumnName("RESULT_CODE");
            entity.Property(e => e.ResultId).HasColumnName("RESULT_ID");
            entity.Property(e => e.ResultMessage)
                .HasMaxLength(500)
                .HasColumnName("RESULT_MESSAGE");
        });

        modelBuilder.Entity<TbRmsResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TB_RMS_R__A61560380456AC35");

            entity.ToTable("TB_RMS_RESULT");

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

            entity.HasOne(d => d.Equip).WithMany(p => p.TbRmsResults)
                .HasForeignKey(d => d.EquipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RESULT_EQUIP");

            entity.HasOne(d => d.Recipe).WithMany(p => p.TbRmsResults)
                .HasForeignKey(d => d.RecipeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RESULT_RECIPE");
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
