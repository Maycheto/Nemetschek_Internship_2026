namespace Services.Dtos.Registration;

public class CompleteSetPasswordRequest
{
    public string Token { get; set; } = null!;

    public string Password { get; set; } = null!;
}
