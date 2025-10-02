using System.Diagnostics;
using Xunit;
using FluentAvalonia.UI.Controls;
using LeigodPauseTool.ViewModels;
using LeigodPauseTool.Services;
using Xunit.Abstractions;

namespace LeigodPauseTool.Tests;

public class UnitTest1(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task LeigodApiServiceTest()
    {
        var service = new ApiService();
        const string token = "M1ttizhVGxdkTehGJ7foIvU5Ku1QqXGO32cE8smBAskvZRhEOKg24jmaF9yYX2eJ";
        var result = await service.Info(token);
        testOutputHelper.WriteLine(result.ToString());
        var result2 = await service.Pause(token);
        testOutputHelper.WriteLine(result2.ToString());
        result = await service.Info(token);
        testOutputHelper.WriteLine(result.ToString());
    }
    [Fact]
    public void TestMethod1()
    {        // 使用 AppleScript 通过 osascript 获取前台有窗口的应用
        Process p = new Process();
        p.StartInfo.FileName = "osascript";
        p.StartInfo.Arguments = "-e \"tell application \\\"System Events\\\" to get name of every process whose background only is false\"";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();

        testOutputHelper.WriteLine("正在运行的应用程序（macOS）：");
        testOutputHelper.WriteLine(output);
    }
}
