using Microsoft.EntityFrameworkCore;

namespace YsEdu.Core.Data;

/// <summary>
/// DbContext를 작업 1건마다 새로 만든다.
/// DbContext는 스레드에 안전하지 않으므로, 메시지 1건 / 조회 1회마다 생성하고 using으로 바로 폐기한다.
/// (2차 과제: "동일 DbContext의 동시 사용을 방지한다")
/// </summary>
public sealed class DbFactory
{
    private readonly DbContextOptions<YsEduDbContext> _options;

    public DbFactory(string connectionString)
    {
        _options = new DbContextOptionsBuilder<YsEduDbContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    public YsEduDbContext Create() => new(_options);
}
