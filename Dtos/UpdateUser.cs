using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Dtos;

public class UpdateUser
{
    [StringLength(100, MinimumLength = 5)]
    public string? Name { get; set; }

    [EmailAddress, StringLength(100, MinimumLength = 5)]
    public string? Email { get; set; }

    [
        StringLength(100, MinimumLength = 7),
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$")
    ]
    public string? Password { get; set; }

    [Phone]
    public string? Phone { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }
}
