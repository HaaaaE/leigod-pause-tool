using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LeigodPauseTool.Models;

namespace LeigodPauseTool.Services;

public class ConfigService
{
    private readonly string _configFilePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ConfigService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        appDataPath = "";
        var appFolder = Path.Combine(appDataPath, "LeigodPauseTool");
        Directory.CreateDirectory(appFolder);
        _configFilePath = Path.Combine(appFolder, "config.json");
    }

    public async Task<Config> LoadConfigAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                return new Config();
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            return JsonSerializer.Deserialize<Config>(json, JsonOptions) ?? new Config();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading config: {ex.Message}");
            return new Config();
        }
    }

    public async Task SaveConfigAsync(Config config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            await File.WriteAllTextAsync(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving config: {ex.Message}");
        }
    }
}