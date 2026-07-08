namespace Services.Options;

public sealed class SmtpOptions
{
    public SmtpOptions() { }
    public SmtpOptions(string smtphost, int port, string username, string password, string email, string name)
    {
        Host = smtphost;
        Port = port;
        Username = username;
        Password = password;
        FromEmail = email;
        FromName = name;
    }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}