using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RmsKafkaMiddleware.Data;

namespace RmsKafkaMiddleware.Common;

/// <summary>
/// 설정 — Topic 및 연결정보는 설정파일(appsettings.json)에서 관리한다 (Kafka p.5)
/// </summary>
public sealed class AppSettings
{
    public required string ConnectionString { get; init; }
    public required string BootstrapServers { get; init; }
    public required string RequestTopic { get; init; }
    public required string ResponseTopic { get; init; }
    public required string GroupId { get; init; }
    public required string SupportedRule { get; init; }
    public required string LogDirectory { get; init; }

    public static AppSettings Load()
    {
        var c = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        string Get(string key) => c[key] ?? throw new InvalidOperationException($"appsettings.json에 {key}가 없습니다.");
        return new AppSettings
        {
            ConnectionString = c.GetConnectionString("YsRmsDB") ?? throw new InvalidOperationException("ConnectionStrings:YsRmsDB가 없습니다."),
            BootstrapServers = Get("Kafka:BootstrapServers"),
            RequestTopic = Get("Kafka:RequestTopic"),
            ResponseTopic = Get("Kafka:ResponseTopic"),
            GroupId = Get("Kafka:GroupId"),
            SupportedRule = Get("Rms:SupportedRule"),
            LogDirectory = Path.Combine(AppContext.BaseDirectory, c["Logging:FileDirectory"] ?? "logs")
        };
    }

    /// DbContext는 메시지 1건마다 새로 만들고 using으로 폐기한다
    public RmsDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<RmsDbContext>().UseSqlServer(ConnectionString).Options);
}
