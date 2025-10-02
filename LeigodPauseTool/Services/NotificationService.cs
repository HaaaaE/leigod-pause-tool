using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;

namespace LeigodPauseTool.Services;

public class NotificationService : INotificationService
{
    public void ShowLoginSuccess(string message)
    {
        SendNotification("登录成功", $"已获取账号令牌: {message}", NotificationType.Success);
    }

    public void ShowLoginCancelled()
    {
        SendNotification("登录取消", "用户取消了登录操作", NotificationType.Warning);
    }

    public void ShowLoginValid()
    {
        SendNotification("登录有效", "当前登录有效", NotificationType.Success);
    }

    public void ShowLoginInvalid()
    {
        SendNotification("未登录/登录失效", "请登录或重新登录", NotificationType.Warning);
    }

    public void ShowPauseSuccess()
    {
        SendNotification("暂停成功", "暂停成功", NotificationType.Success);
    }

    private static void SendNotification(string title, string message, NotificationType type)
    {
        WeakReferenceMessenger.Default.Send(new Notification(
            title,
            message,
            type,
            TimeSpan.FromSeconds(5),
            onClick: null,
            onClose: null)
        );
    }
}