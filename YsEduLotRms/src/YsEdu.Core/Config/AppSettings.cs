using Microsoft.Extensions.Configuration;

namespace YsEdu.Core.Config;

/// <summary>
/// 설정값. DB 연결과 Kafka Topic은 코드에 쓰지 않고 appsettings.json에서 읽는다.
/// (설정 위치: 솔루션 루트 config/appsettings.json → 각 프로젝트 출력 폴더로 복사됨)
/// </summary>
public sealed class AppSettings
{
    public required string ConnectionString { get; init; }
    public required string BootstrapServers { get; init; }
    public required string RequestTopic { get; init; }
    public required string ResponseTopic { get; init; }
    public required string MiddlewareGroupId { get; init; }
    public required string SimulatorGroupId { get; init; }
    public required string LogDirectory { get; init; }
    public int ResponseTimeoutSeconds { get; init; } = 15;

    public static AppSettings Load(string? baseDirectory = null)
    {
        var dir = baseDirectory ?? AppContext.BaseDirectory;
        var config = new ConfigurationBuilder()
            .SetBasePath(dir)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        return new AppSettings
        {
            ConnectionString = config.GetConnectionString("YsEduLotDB")
                               ?? throw new InvalidOperationException("ConnectionStrings:YsEduLotDB 설정이 없습니다."),
            BootstrapServers = Required(config, "Kafka:BootstrapServers"),
            RequestTopic = Required(config, "Kafka:RequestTopic"),
            ResponseTopic = Required(config, "Kafka:ResponseTopic"),
            MiddlewareGroupId = Required(config, "Kafka:MiddlewareGroupId"),
            SimulatorGroupId = Required(config, "Kafka:SimulatorGroupId"),
            ResponseTimeoutSeconds = int.TryParse(config["Kafka:ResponseTimeoutSeconds"], out var t) ? t : 15,
            LogDirectory = Path.Combine(dir, config["Logging:FileDirectory"] ?? "logs")
        };
    }

    private static string Required(IConfiguration config, string key)
        => config[key] ?? throw new InvalidOperationException($"{key} 설정이 없습니다.");
}
