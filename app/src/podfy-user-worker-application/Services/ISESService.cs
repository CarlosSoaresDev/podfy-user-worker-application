namespace podfy_user_worker_application.Services;
public interface ISESService
{
    Task<bool> SendEmailAsync(string body);
}