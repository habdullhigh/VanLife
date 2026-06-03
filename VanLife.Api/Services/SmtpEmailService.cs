using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VanLife.Api.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration config;

    public SmtpEmailService(IConfiguration config)
    {
        this.config = config;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Lightweight implementation that uses SmtpClient if configured, otherwise writes to console.
        var smtpHost = config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            Console.WriteLine($"[Email] To: {to}\nSubject: {subject}\nBody: {body}\n");
            return Task.CompletedTask;
        }

        var message = new MailMessage();
        message.To.Add(to);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = false;
        message.From = new MailAddress(config["Smtp:From"] ?? "no-reply@vanlife.local");

        using var client = new SmtpClient(smtpHost)
        {
            Port = int.TryParse(config["Smtp:Port"], out var p) ? p : 25,
            EnableSsl = bool.TryParse(config["Smtp:EnableSsl"], out var ssl) && ssl
        };

        if (!string.IsNullOrWhiteSpace(config["Smtp:Username"]) && !string.IsNullOrWhiteSpace(config["Smtp:Password"]))
        {
            client.Credentials = new System.Net.NetworkCredential(config["Smtp:Username"], config["Smtp:Password"]);
        }

        return client.SendMailAsync(message);
    }
}
