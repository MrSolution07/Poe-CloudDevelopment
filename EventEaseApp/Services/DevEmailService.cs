namespace EventEaseApp.Services;

/// <summary>
/// Development email service: logs to console and returns the confirmation link.
/// In production, replace with SMTP/SendGrid/Azure Communication Services.
/// </summary>
public class DevEmailService : IEmailService
{
    private readonly ILogger<DevEmailService> _logger;

    public DevEmailService(ILogger<DevEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendConfirmationEmailAsync(string email, string confirmationLink)
    {
        _logger.LogInformation("[DEV EMAIL] Confirmation for {Email}: {Link}", email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        _logger.LogInformation("[DEV EMAIL] Password reset for {Email}: {Link}", email, resetLink);
        return Task.CompletedTask;
    }
}
