using System.Net;
using System.Net.Mail;
using Entities.Enums;
using Microsoft.Extensions.Options;
using Services.Dtos.Registration;
using Services.Interfaces.Registration;
using Web.Options;

namespace Web.Services.Email;

public class SmtpRegistrationEmailSender : IRegistrationEmailSender
{
    private readonly RegistrationEmailOptions options;

    public SmtpRegistrationEmailSender(IOptions<RegistrationEmailOptions> options)
    {
        this.options = options.Value;
    }

    public async Task SendInvitationAsync(
        RegistrationEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureConfigured();

        using var message = new MailMessage
        {
            From = new MailAddress(options.FromEmail, options.FromName),
            Subject = GetSubject(request),
            Body = GetBody(request),
            IsBodyHtml = false
        };
        message.To.Add(request.Email);

        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort)
        {
            EnableSsl = options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(options.SmtpUserName))
        {
            client.Credentials = new NetworkCredential(options.SmtpUserName, options.SmtpPassword);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
    }

    private string GetSubject(RegistrationEmailRequest request)
    {
        return request.Type == RegistrationInvitationType.ParentSignUp
            ? "Complete your NemeBook parent registration"
            : "Set your NemeBook password";
    }

    private string GetBody(RegistrationEmailRequest request)
    {
        var link = BuildInvitationLink(request);
        var actionText = request.Type == RegistrationInvitationType.ParentSignUp
            ? "complete your parent registration"
            : "set your password";

        return $"""
               Hello,

               Use the link below to {actionText}:
               {link}

               This link expires at {request.ExpiresAtUtc:yyyy-MM-dd HH:mm} UTC.
               """;
    }

    private string BuildInvitationLink(RegistrationEmailRequest request)
    {
        var path = request.Type == RegistrationInvitationType.ParentSignUp
            ? options.ParentSignUpPath
            : options.SetPasswordPath;

        var baseUrl = options.BaseUrl.TrimEnd('/');
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var separator = normalizedPath.Contains('?') ? '&' : '?';

        return $"{baseUrl}{normalizedPath}{separator}token={Uri.EscapeDataString(request.Token)}";
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException("Registration email BaseUrl is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("Registration email FromEmail is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            throw new InvalidOperationException("Registration email SmtpHost is not configured.");
        }
    }
}
