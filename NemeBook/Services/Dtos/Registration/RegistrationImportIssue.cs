namespace Services.Dtos.Registration;

public class RegistrationImportIssue
{
    public int? RowNumber { get; set; }

    public string? Email { get; set; }

    public string Message { get; set; } = null!;
}
