using Microsoft.EntityFrameworkCore;
using fdcapp.Models;

namespace fdcapp.Data
{
    /// <summary>
    /// EF Core DbContext - 로컬 MSSQL(TrainingDB) 연결
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>설비 마스터</summary>
        public DbSet<Equipment> Equipments => this.Set<Equipment>();

        /// <summary>설비 로그</summary>
        public DbSet<EquipLog> EquipLogs => this.Set<EquipLog>();

        /// <summary>파라미터 임계치 규칙</summary>
        public DbSet<ParamLimit> ParamLimits => this.Set<ParamLimit>();

        /// <summary>설비 센서 수집 데이터</summary>
        public DbSet<EquipData> EquipDatas => this.Set<EquipData>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost;Database=TrainingDB;User Id=test;Password=1234;TrustServerCertificate=True;");
            }
        }
    }
}