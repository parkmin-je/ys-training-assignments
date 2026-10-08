namespace RmsKafkaMiddleware.Common;

/// <summary>
/// 파일 로그 — DB 오류처럼 DB에 남길 수 없는 오류도 기록한다. 실행 폴더\logs\RmsMiddleware_yyyyMMdd.log
/// </summary>
public sealed class FileLog(string directory)
{
    private readonly object _lock = new();

    /// 화면 표시용 (호출한 스레드에서 발생)
    public event Action<string>? Written;

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(directory);
                File.AppendAllText(Path.Combine(directory, $"RmsMiddleware_{DateTime.Now:yyyyMMdd}.log"), line + Environment.NewLine);
            }
        }
        catch (IOException)
        {
        }
        Written?.Invoke(line);
    }
}
