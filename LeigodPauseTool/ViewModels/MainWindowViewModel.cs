using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentAvalonia.UI.Controls;
using LeigodPauseTool.Views;
using LeigodPauseTool.Models;
namespace LeigodPauseTool.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _detectButtonContent = "开始";

    [RelayCommand]
    private async Task OnGetTokenButtonClick()
    {
        Console.WriteLine("Button clicked");
        
        var loginWindow = new LoginWindow();
        var mainWindow = Application.Current.ApplicationLifetime 
            is IClassicDesktopStyleApplicationLifetime desktop 
            ? desktop.MainWindow 
            : null;
        if (mainWindow != null)
        {
            var result = await loginWindow.ShowDialog<AccountToken?>(mainWindow);
            if (result is null)
            {
                WeakReferenceMessenger.Default.Send(new Notification(
                    "登录取消",
                    "用户取消了登录操作",
                    NotificationType.Warning,
                    TimeSpan.FromSeconds(5),
                    onClick: null,
                    onClose: null)
                );
            }
            else
            {
                Console.WriteLine($"Result from second window: {result}"); // 输出结果
                WeakReferenceMessenger.Default.Send(new Notification(
                    "登录成功",
                    $"已获取账号令牌: {result}",
                    NotificationType.Success,
                    TimeSpan.FromSeconds(5),
                    onClick: null,
                    onClose: null)
                );
            }
        }

    }

    [RelayCommand]
    private void OnDetectButtonClick()
    {
        DetectButtonContent = "检测中";
    }
}
