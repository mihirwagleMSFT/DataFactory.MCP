namespace DataFactory.MCP.Abstractions.Interfaces;

/// <summary>
/// Service for showing native system notifications (toast/banner notifications)
/// in a cross-platform way.
/// </summary>
/// <remarks>
/// This service provides a platform-independent way to show system-level notifications
/// when background tasks complete. The actual notification mechanism varies by platform:
/// - Windows: Windows Toast Notifications via PowerShell
/// - macOS: AppleScript osascript notifications
/// - Linux: notify-send command (libnotify)
/// </remarks>
public interface ISystemNotificationService
{
    /// <summary>
    /// Gets a value indicating whether system notifications are supported on the current platform.
    /// </summary>
    bool IsSupported { get; }

    /// <summary>
    /// Gets or sets a value indicating whether system notifications are enabled.
    /// When disabled, no notifications will be shown even if the platform supports them.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Shows a system notification with the specified title and message.
    /// </summary>
    /// <param name="title">The notification title</param>
    /// <param name="message">The notification body message</param>
    /// <param name="notificationType">The type of notification for visual styling</param>
    /// <returns>A task that completes when the notification request has been sent</returns>
    Task ShowNotificationAsync(string title, string message, NotificationType notificationType = NotificationType.Information);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    Task ShowSuccessAsync(string title, string message);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    Task ShowWarningAsync(string title, string message);
}

/// <summary>
/// Types of system notifications for visual styling.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Informational notification.
    /// </summary>
    Information,

    /// <summary>
    /// Success notification.
    /// </summary>
    Success,

    /// <summary>
    /// Warning notification.
    /// </summary>
    Warning,

    /// <summary>
    /// Error notification.
    /// </summary>
    Error
}
