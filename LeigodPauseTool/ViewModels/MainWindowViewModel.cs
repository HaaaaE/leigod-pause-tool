using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
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
using LeigodPauseTool.Services;

namespace LeigodPauseTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _monitorButtonContent = "开始";
    [ObservableProperty] private int _intervalTime = 60;
    [ObservableProperty] private string _newMonitoredProcess = string.Empty;

    private readonly ConfigService _configService;
    private readonly IApiService _leigodApiService;
    private AccountToken? _accountToken;
    public ObservableCollection<string> MonitoredProcesses { get; set; } = [];

    private Timer? _monitorTimer;

    public MainWindowViewModel()
    {
        _configService = new ConfigService();
        _leigodApiService = new ApiService();
        _ = LoadConfigAsync();
    }

    [RelayCommand]
    private async Task GetAccountTokenAsync()
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
                _accountToken = result;
                _ = SaveConfigAsync();
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
    private void Monitor()
    {
        if (MonitorButtonContent == "开始")
        {
            MonitorButtonContent = "停止";
            StartTimer();
        }
        else
        {
            MonitorButtonContent = "开始";
            StopTimer();
        }
    }

    [RelayCommand]
    private void AddMonitoredProcess()
    {
        MonitoredProcesses.Add(NewMonitoredProcess);
        NewMonitoredProcess = string.Empty;
        _ = SaveConfigAsync();
    }

    [RelayCommand]
    private void RemoveMonitoredProcess(string processName)
    {
        MonitoredProcesses.Remove(processName);
        _ = SaveConfigAsync();
    }


    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // 这里执行您需要的检测逻辑
        Console.WriteLine($"Timer elapsed at: {e.SignalTime}");

        // 在这里添加您的检测代码
        // 注意：这个方法在后台线程池线程上执行
        // 如果需要更新 UI，需要使用 Dispatcher
    }


    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();

        foreach (var process in config.MonitoredProcesses)
        {
            MonitoredProcesses.Add(process);
        }

        _accountToken = config.AccountToken;

        if (_accountToken is not null)
        {
            var pauseStatus = await _leigodApiService.Info(_accountToken.Token);
            if (pauseStatus is PauseStatus.NoAuth)
            {
                WeakReferenceMessenger.Default.Send(new Notification(
                    "登录失效",
                    "请重新登录",
                    NotificationType.Warning,
                    TimeSpan.FromSeconds(5),
                    onClick: null,
                    onClose: null)
                );
                _accountToken = null;
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new Notification(
                    "登录有效",
                    "当前登录有效",
                    NotificationType.Success,
                    TimeSpan.FromSeconds(5),
                    onClick: null,
                    onClose: null)
                );
            }
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new Notification(
                "尚未登录",
                "请登录账号",
                NotificationType.Warning,
                TimeSpan.FromSeconds(5),
                onClick: null,
                onClose: null)
            );
        }

        IntervalTime = config.IntervalTime;
    }

    private async Task SaveConfigAsync()
    {
        var config = new Config
        {
            AccountToken = _accountToken,
            IntervalTime = IntervalTime,
            MonitoredProcesses = MonitoredProcesses.ToList()
        };

        await _configService.SaveConfigAsync(config);
    }

    partial void OnIntervalTimeChanged(int value)
    {
        _ = value;
        _ = SaveConfigAsync();
    }


    private void StartTimer()
    {
        // 如果 timer 已存在，先停止
        StopTimer();

        // 创建新的 timer，间隔时间为 intervalTime 秒
        _monitorTimer = new Timer(IntervalTime * 1000);
        _monitorTimer.Elapsed += OnTimerElapsed;
        _monitorTimer.AutoReset = true; // 自动重复
        _monitorTimer.Start();

        Console.WriteLine($"Timer started with interval: {IntervalTime} seconds");
    }

    private void StopTimer()
    {
        if (_monitorTimer != null)
        {
            _monitorTimer.Stop();
            _monitorTimer.Elapsed -= OnTimerElapsed;
            _monitorTimer.Dispose();
            _monitorTimer = null;
            Console.WriteLine("Timer stopped");
        }
    }
}