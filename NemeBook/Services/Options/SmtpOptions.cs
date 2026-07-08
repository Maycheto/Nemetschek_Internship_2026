namespace Services.Options;

public sealed class SmtpOptions
{
    private SmtpOptions(){}
    public SmtpOptions(string smtphost, int port, string username, string password, string email, string name)
    {
        Host = smtphost;
        Port = port;
        Username = username;
        Password = password;
        FromEmail = email;
        FromName = name;
    }
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}