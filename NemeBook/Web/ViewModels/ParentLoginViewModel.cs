using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels;

public class ParentLoginViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
