namespace LeigodPauseTool.Services;

public interface INotificationService
{
    void ShowLoginSuccess(string message);
    void ShowLoginCancelled();
    void ShowLoginValid();
    void ShowLoginInvalid();
    void ShowPauseSuccess();
}