using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LeigodPauseTool.Views;
using LeigodPauseTool.Models;
using LeigodPauseTool.Services;

namespace LeigodPauseTool.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _monitorButtonContent = "开始";
    [ObservableProperty] private int _intervalTime = 60;
    [ObservableProperty] private string _newMonitoredProcess = string.Empty;
    [ObservableProperty] private bool _isMonitoring;
    private int _countdownSeconds;

    public int CountdownSeconds
    {
        get => _countdownSeconds;
        set
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _countdownSeconds = value switch
                {
                    > 1 => value,
                    -1 => 0,
                    _ => 1
                };
                OnPropertyChanged(nameof(CountdownSeconds));
            });
        }
    }

    private readonly ConfigService _configService;
    private readonly IApiService _leigodApiService;
    private readonly INotificationService _notificationService;
    private AccountToken? _accountToken;
    public ObservableCollection<string> MonitoredProcesses { get; set; } = [];

    private Timer? _monitorTimer;

    public MainWindowViewModel()
    {
        _configService = new ConfigService();
        _leigodApiService = new ApiService();
        _notificationService = new NotificationService();
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
                _notificationService.ShowLoginCancelled();
            }
            else
            {
                Console.WriteLine($"Result from second window: {result}"); // 输出结果
                _accountToken = result;
                _ = SaveConfigAsync();
                _notificationService.ShowLoginSuccess(result.ToString());
            }
        }
    }

    [RelayCommand]
    private async Task Monitor()
    {
        if (!await IsAccountTokenValidAsync())
        {
            _notificationService.ShowLoginInvalid();
            _accountToken = null;
            return;
        }

        if (MonitorButtonContent == "开始")
        {
            StartTimer();
        }
        else
        {
            StopTimer();
        }
    }

    [RelayCommand]
    private async Task CheckAccountTokenValidityAsync()
    {
        if (!await IsAccountTokenValidAsync())
        {
            _notificationService.ShowLoginInvalid();
            _accountToken = null;
        }
        else
        {
            _notificationService.ShowLoginValid();
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
        // 倒计时递减
        CountdownSeconds--;

        // 如果倒计时未归零，继续等待
        if (CountdownSeconds > 1)
        {
            return;
        }

        // 倒计时归零，执行检测逻辑
        Console.WriteLine($"Timer elapsed at: {e.SignalTime}");

        // 重置倒计时
        CountdownSeconds = IntervalTime;

        // 检测被监控的进程是否运行
        if (AnyMonitoredProcessRunning()) return;

        StopTimer();
        if (_accountToken is null)
        {
            _notificationService.ShowLoginInvalidWithSystemToast();
            return;
        }

        var res = _leigodApiService.Pause(_accountToken.Token).Result;
        Console.WriteLine(res);
        if (res.Code != 0 && res.Code != 400803)
        {
            _notificationService.ShowLoginInvalidWithSystemToast();
            _accountToken = null;
            return;
        }

        _notificationService.ShowPauseSuccess();
    }


    private async Task LoadConfigAsync()
    {
        var config = await _configService.LoadConfigAsync();

        foreach (var process in config.MonitoredProcesses)
        {
            MonitoredProcesses.Add(process);
        }

        _accountToken = config.AccountToken;


        if (await IsAccountTokenValidAsync())
        {
            _notificationService.ShowLoginValid();
        }
        else
        {
            _notificationService.ShowLoginInvalid();
            _accountToken = null;
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

    private async Task<bool> IsAccountTokenValidAsync()
    {
        if (_accountToken is null)
            return false;

        var res = await _leigodApiService.Info(_accountToken.Token);
        return res is not PauseStatus.NoAuth;
    }

    private bool AnyMonitoredProcessRunning()
    {
        var runningProcessesWithWindows = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Select(p => p.ProcessName)
            .ToList();

        return MonitoredProcesses.Any(monitoredProcess =>
            runningProcessesWithWindows.Contains(monitoredProcess, StringComparer.OrdinalIgnoreCase));
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
        MonitorButtonContent = "停止";
        IsMonitoring = true;

        // 初始化倒计时
        CountdownSeconds = IntervalTime;

        // 创建 timer，每秒触发一次
        _monitorTimer = new Timer(1000);
        _monitorTimer.Elapsed += OnTimerElapsed;
        _monitorTimer.AutoReset = true; // 自动重复
        _monitorTimer.Start();

        Console.WriteLine($"Timer started with interval: {IntervalTime} seconds");
    }

    private void StopTimer()
    {
        MonitorButtonContent = "开始";
        IsMonitoring = false;
        CountdownSeconds = -1;

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