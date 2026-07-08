using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.Options;
using Services.Services.Email;

var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "nemebook1@gmail.com";
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "tfcg elkb mgrg unks";

if (string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
{
    Console.WriteLine("ERROR: SMTP_USERNAME and SMTP_PASSWORD environment variables must be set");
    Console.WriteLine("Usage: SMTP_USERNAME='you@gmail.com' SMTP_PASSWORD='app-password' dotnet run");
    Environment.Exit(1);
}

var config = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        { "Email:SmtpHost", "smtp.gmail.com" },
        { "Email:SmtpPort", "587" },
        { "Email:SmtpUsername", smtpUsername },
        { "Email:SmtpPassword", smtpPassword },
        { "Email:FromEmail", smtpUsername },
        { "Email:FromName", "NemeBook" }
    })
    .Build();

Console.WriteLine($"Using SMTP account: {smtpUsername}");

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<EmailService>();

try
{
    var registrationOptions = Options.Create(new RegistrationEmailOptions
    {
        BaseUrl = "http://localhost:5000",
        ParentSignUpPath = "/auth/register/parent",
        SetPasswordPath = "/auth/register/teacher"
    });

    var emailService = new EmailService(config, logger, registrationOptions);

    Console.WriteLine("\nSending test email...");
    await emailService.SendNotificationEmailAsync(
        "boris.velkov.highschool@buditel.bg",
        "Jane Doe",
        "Test Notification",
        "This is a test email from NemeBook. Sent at " + DateTime.Now
    );

    Console.WriteLine("✓ Email sent successfully!");
    Console.WriteLine("\nCheck the inbox: borkoaxtyt@gmail.com");
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗ Error sending email: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner error: {ex.InnerException.Message}");
    }
    Environment.Exit(1);
}