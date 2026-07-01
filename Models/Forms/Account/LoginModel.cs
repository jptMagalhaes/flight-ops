using System.ComponentModel.DataAnnotations;

namespace FlightOps.Models.Forms.Account;

public class LoginModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
