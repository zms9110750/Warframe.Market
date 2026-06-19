using System.IO;
using System.Runtime.CompilerServices;

namespace WarframeMarketApp.Services;

/// <summary>
/// 简易文件日志。写 app.log。
/// </summary>
public class FileLogger
{
	private readonly string _path;

	public FileLogger()
	{
		_path = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"WarframeMarket", "app.log");
		var dir = Path.GetDirectoryName(_path);
		if (dir != null) Directory.CreateDirectory(dir);
		// 启动新会话标记
		WriteLine($"=== 会话启动 {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
	}

	public void Info(string msg, [CallerMemberName] string? member = null)
	{
		WriteLine($"[INF] {member}: {msg}");
	}

	public void Warn(string msg, [CallerMemberName] string? member = null)
	{
		WriteLine($"[WRN] {member}: {msg}");
	}

	public void Error(string msg, Exception? ex = null, [CallerMemberName] string? member = null)
	{
		WriteLine($"[ERR] {member}: {msg}");
		if (ex != null) WriteLine($"  {ex.GetType().Name}: {ex.Message}");
	}

	private void WriteLine(string line)
	{
		try
		{
			File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss.fff} {line}\n");
		}
		catch { /* 日志不能崩 */ }
	}
}
