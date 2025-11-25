using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OptiScaler.Core.Services;

/// <summary>
/// Service for handling application crashes and generating crash reports
/// </summary>
public class CrashReportService
{
    private static readonly string CrashLogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OptiScaler",
        "CrashLogs"
    );

    public CrashReportService()
    {
        Directory.CreateDirectory(CrashLogDirectory);
    }

    /// <summary>
    /// Logs an unhandled exception to a crash report file
    /// </summary>
    public async Task<string> LogCrashAsync(Exception exception, string additionalInfo = "")
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var crashFileName = $"crash_{timestamp}.log";
            var crashFilePath = Path.Combine(CrashLogDirectory, crashFileName);

            var crashReport = BuildCrashReport(exception, additionalInfo);

            await File.WriteAllTextAsync(crashFilePath, crashReport, Encoding.UTF8);

            return crashFilePath;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Builds a detailed crash report from an exception
    /// </summary>
    private string BuildCrashReport(Exception exception, string additionalInfo)
    {
        var sb = new StringBuilder();

        sb.AppendLine("???????????????????????????????????????????????????????????");
        sb.AppendLine($"  OptiScaler Manager - Crash Report");
        sb.AppendLine("???????????????????????????????????????????????????????????");
        sb.AppendLine();
        sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Version: {AppInfo.Version}");
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($".NET: {Environment.Version}");
        sb.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(additionalInfo))
        {
            sb.AppendLine("Additional Information:");
            sb.AppendLine(additionalInfo);
            sb.AppendLine();
        }

        sb.AppendLine("Exception Details:");
        sb.AppendLine("???????????????????????????????????????????????????????????");
        
        var currentException = exception;
        var level = 0;

        while (currentException != null)
        {
            if (level > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Inner Exception (Level {level}):");
                sb.AppendLine("???????????????????????????????????????????????????????????");
            }

            sb.AppendLine($"Type: {currentException.GetType().FullName}");
            sb.AppendLine($"Message: {currentException.Message}");
            sb.AppendLine($"Source: {currentException.Source}");
            
            if (!string.IsNullOrEmpty(currentException.StackTrace))
            {
                sb.AppendLine();
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(currentException.StackTrace);
            }

            if (currentException.Data.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Additional Data:");
                foreach (var key in currentException.Data.Keys)
                {
                    sb.AppendLine($"  {key}: {currentException.Data[key]}");
                }
            }

            currentException = currentException.InnerException;
            level++;
        }

        sb.AppendLine();
        sb.AppendLine("???????????????????????????????????????????????????????????");
        sb.AppendLine("  End of Crash Report");
        sb.AppendLine("???????????????????????????????????????????????????????????");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the crash logs directory path
    /// </summary>
    public string GetCrashLogsDirectory() => CrashLogDirectory;

    /// <summary>
    /// Gets all crash log files
    /// </summary>
    public string[] GetCrashLogs()
    {
        try
        {
            if (Directory.Exists(CrashLogDirectory))
            {
                return Directory.GetFiles(CrashLogDirectory, "crash_*.log");
            }
        }
        catch
        {
            // Ignore
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Cleans up old crash logs (keeps last 10)
    /// </summary>
    public async Task CleanupOldLogsAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var logs = GetCrashLogs();
                if (logs.Length > 10)
                {
                    var sortedLogs = logs.OrderByDescending(f => new FileInfo(f).CreationTime).ToArray();
                    
                    for (int i = 10; i < sortedLogs.Length; i++)
                    {
                        File.Delete(sortedLogs[i]);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        });
    }
}
