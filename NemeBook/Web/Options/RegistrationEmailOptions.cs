namespace Web.Options;

public class RegistrationEmailOptions
{
    public string BaseUrl { get; set; } = null!;

    public string SetPasswordPath { get; set; } = "/Account/SetPassword";

    public string ParentSignUpPath { get; set; } = "/Account/ParentSignUp";

    public string FromEmail { get; set; } = null!;

    public string FromName { get; set; } = "NemeBook";

    public string SmtpHost { get; set; } = null!;

    public int SmtpPort { get; set; } = 587;

    public string? SmtpUserName { get; set; }

    public string? SmtpPassword { get; set; }

    public bool EnableSsl { get; set; } = true;
}
