using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Text.Json;
using System.Web;
using Avalonia.Controls.Notifications;
using LeigodPauseTool.Models;
using Xilium.CefGlue;

namespace LeigodPauseTool.Views;

public partial class LoginWindow : Window
{
    private DispatcherTimer? _timer;

    public LoginWindow()
    {
        InitializeComponent();
        // 获取默认 Cookie 管理器
        var cookieManager = CefCookieManager.GetGlobal(null);
        cookieManager.DeleteCookies(null, null, null);
        // 窗口打开后启动定时器
        this.Opened += OnWindowOpened;
        // 窗口关闭时清理定时器
        this.Closed += OnWindowClosed;
    }


    private void OnWindowOpened(object? sender, EventArgs e)
    {
        // 创建定时器
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1) // 设置刷新间隔，这里是1秒
        };

        // 订阅 Tick 事件
        _timer.Tick += OnTimerTick;

        // 启动定时器
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        // 在这里执行您需要循环刷新的函数
        _ = GetToken();
    }

    private async Task GetToken()
    {
        Console.WriteLine($"刷新执行 - {DateTime.Now}");
        var cookie = await Browser.EvaluateScript<string>("return document.cookie");

        var start = cookie.IndexOf("account_token={", StringComparison.Ordinal);
        var end = cookie.IndexOf("};", start, StringComparison.Ordinal);

        if (start != -1 && end != -1)
        {
            var json = cookie.Substring(start + 14, end - start - 14 + 1); // 跳过 "account_token="
            json = HttpUtility.UrlDecode(json);
            var doc = JsonDocument.Parse(json);

            var token = doc.RootElement.GetProperty("account_token").GetString()!;
            var expiry = doc.RootElement.GetProperty("expiry_time").GetString()!;

            Console.WriteLine(token); // Gn62xNkB1Bp3t2hA0EZVHU9IgYxMxVRPr7hR0Nv5OkMAz5tq1Kao9xxS0ZvPM1XM
            Console.WriteLine(expiry); // 2025-10-09 04:58:02
            Console.WriteLine(cookie);
            Close(new AccountToken { Token = token, ExpiryTime = DateTime.Parse(expiry) });
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        // 停止并释放定时器
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }
}