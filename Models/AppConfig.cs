using System.Collections.Generic;

namespace LeigodPauseTool.Models;

public class AppConfig
{
    public AccountToken? AccountToken { get; set; }
    public List<string> MonitoredProcesses { get; set; } = new();
    public int IntervalTime { get; set; } = 60;
}
