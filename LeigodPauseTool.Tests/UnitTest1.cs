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
    {
        // Get all processes that have a main window
        var processesWithWindows = Process.GetProcesses()
            .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            .Select(p => p.ProcessName)
            .ToList();

        testOutputHelper.WriteLine($"Found {processesWithWindows.Count} processes with windows:\n");
        foreach (var process in processesWithWindows)
        {
            testOutputHelper.WriteLine($"{process}");
        }
    }
}