using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DataFactory.MCP.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;

namespace DataFactory.MCP.Services;

/// <summary>
/// Cross-platform service for showing native system notifications (toast/banner notifications).
/// Uses platform-specific commands (PowerShell on Windows, osascript on macOS, notify-send on Linux).
/// </summary>
public class SystemNotificationService : ISystemNotificationService
{
    private readonly ILogger<SystemNotificationService> _logger;
    private readonly string _xamlTemplate;
    private readonly string _psScriptTemplate;
    private bool _isEnabled = true;

    public SystemNotificationService(ILogger<SystemNotificationService> logger)
    {
        _logger = logger;
        _xamlTemplate = LoadEmbeddedResource("ToastNotification.xaml");
        _psScriptTemplate = LoadEmbeddedResource("ToastNotification.ps1");
    }

    /// <inheritdoc />
    public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                               RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                               RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <inheritdoc />
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <inheritdoc />
    public Task ShowNotificationAsync(string title, string message, NotificationType notificationType = NotificationType.Information)
    {
        if (!IsEnabled)
        {
            _logger.LogDebug("System notifications are disabled, skipping notification: {Title}", title);
            return Task.CompletedTask;
        }

        try
        {
            Show(title, message, notificationType);
            _logger.LogDebug("System notification shown: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show system notification: {Title}", title);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ShowSuccessAsync(string title, string message)
        => ShowNotificationAsync(title, message, NotificationType.Success);

    /// <inheritdoc />
    public Task ShowErrorAsync(string title, string message)
        => ShowNotificationAsync(title, message, NotificationType.Error);

    /// <inheritdoc />
    public Task ShowWarningAsync(string title, string message)
        => ShowNotificationAsync(title, message, NotificationType.Warning);

    private void Show(string title, string message, NotificationType notificationType = NotificationType.Information)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS Notification Center banner
            Run("osascript", $"-e \"display notification \\\"{EscapeAppleScript(message)}\\\" with title \\\"{EscapeAppleScript(title)}\\\"\"");
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Linux libnotify (requires notify-send installed)
            Run("notify-send", $"{Quote(title)} {Quote(message)}");
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Windows custom WPF toast notification
            ShowWindowsToast(title, message, notificationType);
            return;
        }

        _logger.LogDebug("No notification mechanism available for this platform");
    }

    private void ShowWindowsToast(string title, string message, NotificationType notificationType)
    {
        var icon = notificationType switch
        {
            NotificationType.Success => "✅",
            NotificationType.Error => "❌",
            NotificationType.Warning => "⚠️",
            _ => "ℹ️"
        };

        var borderColor = notificationType switch
        {
            NotificationType.Success => "#28A745",
            NotificationType.Error => "#DC3545",
            NotificationType.Warning => "#FFC107",
            _ => "#007ACC"
        };

        // Escape for XAML attributes and apply template substitutions
        var safeTitle = EscapeXml($"{icon} {title}");
        var safeMessage = EscapeXml(message);

        var xamlContent = _xamlTemplate
            .Replace("{{BorderColor}}", borderColor)
            .Replace("{{Title}}", safeTitle)
            .Replace("{{Message}}", safeMessage);

        var psScript = _psScriptTemplate.Replace("{{XamlContent}}", xamlContent);

        try
        {
            // Write script to temp file with UTF-8 BOM encoding for proper Unicode/emoji support in PowerShell
            var scriptPath = Path.Combine(Path.GetTempPath(), $"mcp-toast-{Guid.NewGuid():N}.ps1");
            File.WriteAllText(scriptPath, psScript, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -STA -WindowStyle Hidden -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            // Clean up script file after a delay (fire and forget)
            _ = Task.Delay(10000).ContinueWith(_ =>
            {
                try { File.Delete(scriptPath); } catch { }
            });
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to show Windows toast notification");
        }
    }

    private static string LoadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"DataFactory.MCP.Core.Resources.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private void Run(string file, string args)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = file,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to run notification process: {File}", file);
        }
    }

    private static string EscapeXml(string s) =>
        s.Replace("&", "&amp;")
         .Replace("<", "&lt;")
         .Replace(">", "&gt;")
         .Replace("\"", "&quot;")
         .Replace("'", "&apos;");

    private static string Quote(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

    private static string EscapeAppleScript(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
