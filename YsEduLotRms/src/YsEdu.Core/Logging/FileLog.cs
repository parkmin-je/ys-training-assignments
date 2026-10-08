namespace YsEdu.Core.Logging;

/// <summary>
/// 파일 로그. DB에 저장할 수 없는 오류(DB 연결 실패 등)도 남길 수 있도록 파일에 기록한다.
/// (2차 과제: "DB 저장이 불가능한 오류는 파일 등 별도의 기록 수단을 사용한다")
/// 위치: 실행 폴더\logs\{프로그램}_yyyyMMdd.log
/// </summary>
public sealed class FileLog
{
    private readonly string _directory;
    private readonly string _prefix;
    private readonly object _lock = new();

    public FileLog(string directory, string prefix)
    {
        _directory = directory;
        _prefix = prefix;
    }

    /// 화면 등 다른 곳에서도 같은 로그를 받아 보고 싶을 때 구독한다 (호출 스레드에서 발생)
    public event Action<string>? Written;

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
        try
        {
            lock (_lock)
            {
                Directory.CreateDirectory(_directory);
                File.AppendAllText(Path.Combine(_directory, $"{_prefix}_{DateTime.Now:yyyyMMdd}.log"), line + Environment.NewLine);
            }
        }
        catch (IOException)
        {
            // 파일 기록 실패가 업무 처리를 멈추게 하지 않는다
        }
        Written?.Invoke(line);
    }
}
