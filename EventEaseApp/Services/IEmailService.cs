namespace EventEaseApp.Services;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string email, string confirmationLink);
    Task SendPasswordResetEmailAsync(string email, string resetLink);
}
