using System.Threading.Tasks;

namespace VanLife.Api.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
