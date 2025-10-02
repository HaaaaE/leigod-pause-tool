using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Avalonia.Controls.Notifications;

namespace LeigodPauseTool.Views;

public partial class MainWindow : Window
{
    private WindowNotificationManager _notificationManager;

    public MainWindow()
    {
        InitializeComponent();
        _notificationManager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3
        };
        //_ = Notify();
        WeakReferenceMessenger.Default.Register<Notification>(this, (r, m) =>
        {
            Console.WriteLine($"收到消息: {m.Title}");
            // 这里可以调用 ShowDialog 或更新 UI
            _notificationManager.Show(m);
        });
    }

    private async Task Notify()
    {
        await Task.Delay(1000);
        _notificationManager.Show(new Notification(
                "标题",
                "这是悬浮通知内容",
                NotificationType.Success,
                TimeSpan.FromSeconds(5),
                onClick: null,
                onClose: null
        ));
    }
}