using LeigodPauseTool.Models;
using System.Text.Json;
using System.Threading.Tasks;

namespace LeigodPauseTool.Services;

public interface IApiService
{
    Task<ApiEnvelope> Pause(string accountToken);
    Task<PauseStatus> Info(string accountToken);
}