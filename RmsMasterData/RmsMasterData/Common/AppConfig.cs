using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RmsMasterData.Data;

namespace RmsMasterData.Common;

/// <summary>
/// 설정과 DbContext 생성. 연결 문자열은 appsettings.json에서 읽는다.
/// DbContext는 작업(조회 1회 / 저장 1회)마다 새로 만들고 using으로 폐기한다.
/// </summary>
public static class AppConfig
{
    private static readonly Lazy<string> _connectionString = new(() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build()
            .GetConnectionString("YsRmsDB")
        ?? throw new InvalidOperationException("appsettings.json에 ConnectionStrings:YsRmsDB가 없습니다."));

    public static string ConnectionString => _connectionString.Value;

    public static RmsDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<RmsDbContext>().UseSqlServer(ConnectionString).Options);
}
