using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Data.Entities;

namespace YsEdu.Core.Data;

public partial class YsEduDbContext : DbContext
{
    public YsEduDbContext(DbContextOptions<YsEduDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TbEqpRun> TbEqpRuns { get; set; }

    public virtual DbSet<TbEquipment> TbEquipments { get; set; }

    public virtual DbSet<TbLot> TbLots { get; set; }

    public virtual DbSet<TbLotEvent> TbLotEvents { get; set; }

    public virtual DbSet<TbMsgLog> TbMsgLogs { get; set; }

    public virtual DbSet<TbMsgProcessed> TbMsgProcesseds { get; set; }

    public virtual DbSet<TbProduct> TbProducts { get; set; }

    public virtual DbSet<TbRecipe> TbRecipes { get; set; }

    public virtual DbSet<TbRecipeParam> TbRecipeParams { get; set; }

    public virtual DbSet<TbRmsCheck> TbRmsChecks { get; set; }

    public virtual DbSet<TbRmsCheckDtl> TbRmsCheckDtls { get; set; }

    public virtual DbSet<TbStep> TbSteps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TbEqpRun>(entity =>
        {
            entity.HasKey(e => e.RunId);

            entity.ToTable("TB_EQP_RUN");

            entity.HasIndex(e => new { e.LotId, e.StartTime }, "IX_TB_EQP_RUN_LOT");

            entity.HasIndex(e => new { e.LotId, e.StepId, e.EqpId }, "UX_TB_EQP_RUN_OPEN")
                .IsUnique()
                .HasFilter("([RUN_STATUS]=N'RUN')");

            entity.Property(e => e.RunId).HasColumnName("RUN_ID");
            entity.Property(e => e.CheckId).HasColumnName("CHECK_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EndMsgId)
                .HasMaxLength(50)
                .HasColumnName("END_MSG_ID");
            entity.Property(e => e.EndTime)
                .HasPrecision(3)
                .HasColumnName("END_TIME");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.LotId)
                .HasMaxLength(50)
                .HasColumnName("LOT_ID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
            entity.Property(e => e.RunStatus)
                .HasMaxLength(10)
                .HasColumnName("RUN_STATUS");
            entity.Property(e => e.StartMsgId)
                .HasMaxLength(50)
                .HasColumnName("START_MSG_ID");
            entity.Property(e => e.StartTime)
                .HasPrecision(3)
                .HasColumnName("START_TIME");
            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("UPDATED_AT");
        });

        modelBuilder.Entity<TbEquipment>(entity =>
        {
            entity.HasKey(e => e.EqpId);

            entity.ToTable("TB_EQUIPMENT");

            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.CurLotId)
                .HasMaxLength(50)
                .HasColumnName("CUR_LOT_ID");
            entity.Property(e => e.EqpName)
                .HasMaxLength(100)
                .HasColumnName("EQP_NAME");
            entity.Property(e => e.EqpStatus)
                .HasMaxLength(10)
                .HasDefaultValue("IDLE")
                .HasColumnName("EQP_STATUS");
            entity.Property(e => e.LineId)
                .HasMaxLength(50)
                .HasColumnName("LINE_ID");
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

        modelBuilder.Entity<TbLot>(entity =>
        {
            entity.HasKey(e => e.LotId);

            entity.ToTable("TB_LOT");

            entity.Property(e => e.LotId)
                .HasMaxLength(50)
                .HasColumnName("LOT_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.CurEqpId)
                .HasMaxLength(50)
                .HasColumnName("CUR_EQP_ID");
            entity.Property(e => e.EndMsgId)
                .HasMaxLength(50)
                .HasColumnName("END_MSG_ID");
            entity.Property(e => e.EndTime)
                .HasPrecision(3)
                .HasColumnName("END_TIME");
            entity.Property(e => e.LotStatus)
                .HasMaxLength(10)
                .HasColumnName("LOT_STATUS");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.StartMsgId)
                .HasMaxLength(50)
                .HasColumnName("START_MSG_ID");
            entity.Property(e => e.StartTime)
                .HasPrecision(3)
                .HasColumnName("START_TIME");
            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("UPDATED_AT");
        });

        modelBuilder.Entity<TbLotEvent>(entity =>
        {
            entity.HasKey(e => e.EventSeq);

            entity.ToTable("TB_LOT_EVENT");

            entity.HasIndex(e => new { e.EqpId, e.EventTime }, "IX_TB_LOT_EVENT_EQP");

            entity.HasIndex(e => new { e.LotId, e.EventTime }, "IX_TB_LOT_EVENT_LOT");

            entity.Property(e => e.EventSeq).HasColumnName("EVENT_SEQ");
            entity.Property(e => e.CheckId).HasColumnName("CHECK_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.EventTime)
                .HasPrecision(3)
                .HasColumnName("EVENT_TIME");
            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .HasColumnName("EVENT_TYPE");
            entity.Property(e => e.LotId)
                .HasMaxLength(50)
                .HasColumnName("LOT_ID");
            entity.Property(e => e.MessageId)
                .HasMaxLength(50)
                .HasColumnName("MESSAGE_ID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.Reason)
                .HasMaxLength(400)
                .HasColumnName("REASON");
            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasColumnName("RESULT");
            entity.Property(e => e.RunId).HasColumnName("RUN_ID");
            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
        });

        modelBuilder.Entity<TbMsgLog>(entity =>
        {
            entity.HasKey(e => e.LogSeq);

            entity.ToTable("TB_MSG_LOG");

            entity.HasIndex(e => new { e.MessageId, e.Direction }, "IX_TB_MSG_LOG_MESSAGE");

            entity.Property(e => e.LogSeq).HasColumnName("LOG_SEQ");
            entity.Property(e => e.Direction)
                .HasMaxLength(3)
                .HasColumnName("DIRECTION");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .HasColumnName("EVENT_TYPE");
            entity.Property(e => e.LoggedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("LOGGED_AT");
            entity.Property(e => e.LotId)
                .HasMaxLength(50)
                .HasColumnName("LOT_ID");
            entity.Property(e => e.MessageId)
                .HasMaxLength(50)
                .HasColumnName("MESSAGE_ID");
            entity.Property(e => e.OffsetNo).HasColumnName("OFFSET_NO");
            entity.Property(e => e.PartitionNo).HasColumnName("PARTITION_NO");
            entity.Property(e => e.RawXml).HasColumnName("RAW_XML");
            entity.Property(e => e.Reason)
                .HasMaxLength(400)
                .HasColumnName("REASON");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasColumnName("RESULT");
            entity.Property(e => e.Topic)
                .HasMaxLength(100)
                .HasColumnName("TOPIC");
        });

        modelBuilder.Entity<TbMsgProcessed>(entity =>
        {
            entity.HasKey(e => e.MessageId);

            entity.ToTable("TB_MSG_PROCESSED");

            entity.Property(e => e.MessageId)
                .HasMaxLength(50)
                .HasColumnName("MESSAGE_ID");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .HasColumnName("EVENT_TYPE");
            entity.Property(e => e.ProcessedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("PROCESSED_AT");
            entity.Property(e => e.Reason)
                .HasMaxLength(400)
                .HasColumnName("REASON");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasColumnName("RESULT");
        });

        modelBuilder.Entity<TbProduct>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.ToTable("TB_PRODUCT");

            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("PRODUCT_NAME");
            entity.Property(e => e.UseYn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("USE_YN");
        });

        modelBuilder.Entity<TbRecipe>(entity =>
        {
            entity.HasKey(e => e.RecipeId);

            entity.ToTable("TB_RECIPE");

            entity.HasIndex(e => new { e.ProductId, e.StepId, e.EqpId }, "UQ_TB_RECIPE_CONDITION").IsUnique();

            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasColumnName("UPDATED_AT");
            entity.Property(e => e.UseYn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("USE_YN");

            entity.HasOne(d => d.Eqp).WithMany(p => p.TbRecipes)
                .HasForeignKey(d => d.EqpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_RECIPE_EQUIPMENT");

            entity.HasOne(d => d.Product).WithMany(p => p.TbRecipes)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_RECIPE_PRODUCT");

            entity.HasOne(d => d.Step).WithMany(p => p.TbRecipes)
                .HasForeignKey(d => d.StepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TB_RECIPE_STEP");
        });

        modelBuilder.Entity<TbRecipeParam>(entity =>
        {
            entity.HasKey(e => new { e.RecipeId, e.ParamId });

            entity.ToTable("TB_RECIPE_PARAM");

            entity.Property(e => e.RecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECIPE_ID");
            entity.Property(e => e.ParamId)
                .HasMaxLength(50)
                .HasColumnName("PARAM_ID");
            entity.Property(e => e.BaseValue)
                .HasMaxLength(100)
                .HasColumnName("BASE_VALUE");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");

            entity.HasOne(d => d.Recipe).WithMany(p => p.TbRecipeParams)
                .HasForeignKey(d => d.RecipeId)
                .HasConstraintName("FK_TB_RECIPE_PARAM_RECIPE");
        });

        modelBuilder.Entity<TbRmsCheck>(entity =>
        {
            entity.HasKey(e => e.CheckId);

            entity.ToTable("TB_RMS_CHECK");

            entity.HasIndex(e => new { e.LotId, e.EqpId, e.EventTime }, "IX_TB_RMS_CHECK_LOT");

            entity.Property(e => e.CheckId).HasColumnName("CHECK_ID");
            entity.Property(e => e.BaseRecipeId)
                .HasMaxLength(50)
                .HasColumnName("BASE_RECIPE_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.EqpId)
                .HasMaxLength(50)
                .HasColumnName("EQP_ID");
            entity.Property(e => e.EventTime)
                .HasPrecision(3)
                .HasColumnName("EVENT_TIME");
            entity.Property(e => e.LotId)
                .HasMaxLength(50)
                .HasColumnName("LOT_ID");
            entity.Property(e => e.MessageId)
                .HasMaxLength(50)
                .HasColumnName("MESSAGE_ID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .HasColumnName("PRODUCT_ID");
            entity.Property(e => e.Reason)
                .HasMaxLength(400)
                .HasColumnName("REASON");
            entity.Property(e => e.RecvRecipeId)
                .HasMaxLength(50)
                .HasColumnName("RECV_RECIPE_ID");
            entity.Property(e => e.Result)
                .HasMaxLength(10)
                .HasColumnName("RESULT");
            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
        });

        modelBuilder.Entity<TbRmsCheckDtl>(entity =>
        {
            entity.HasKey(e => e.DtlSeq);

            entity.ToTable("TB_RMS_CHECK_DTL");

            entity.Property(e => e.DtlSeq).HasColumnName("DTL_SEQ");
            entity.Property(e => e.BaseValue)
                .HasMaxLength(100)
                .HasColumnName("BASE_VALUE");
            entity.Property(e => e.CheckId).HasColumnName("CHECK_ID");
            entity.Property(e => e.ItemId)
                .HasMaxLength(50)
                .HasColumnName("ITEM_ID");
            entity.Property(e => e.ItemType)
                .HasMaxLength(10)
                .HasColumnName("ITEM_TYPE");
            entity.Property(e => e.Judge)
                .HasMaxLength(10)
                .HasColumnName("JUDGE");
            entity.Property(e => e.Reason)
                .HasMaxLength(100)
                .HasColumnName("REASON");
            entity.Property(e => e.RecvValue)
                .HasMaxLength(100)
                .HasColumnName("RECV_VALUE");

            entity.HasOne(d => d.Check).WithMany(p => p.TbRmsCheckDtls)
                .HasForeignKey(d => d.CheckId)
                .HasConstraintName("FK_TB_RMS_CHECK_DTL_CHECK");
        });

        modelBuilder.Entity<TbStep>(entity =>
        {
            entity.HasKey(e => e.StepId);

            entity.ToTable("TB_STEP");

            entity.Property(e => e.StepId)
                .HasMaxLength(50)
                .HasColumnName("STEP_ID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("CREATED_AT");
            entity.Property(e => e.StepName)
                .HasMaxLength(100)
                .HasColumnName("STEP_NAME");
            entity.Property(e => e.UseYn)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("USE_YN");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
