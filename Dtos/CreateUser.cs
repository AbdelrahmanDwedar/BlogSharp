using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Dtos;

public class CreateUser
{
    [Required, StringLength(100, MinimumLength = 5)]
    public required string Name { get; set; }

    [Required, EmailAddress, StringLength(100, MinimumLength = 5)]
    public required string Email { get; set; }

    [
        Required,
        StringLength(100, MinimumLength = 7),
        RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*])[A-Za-z\d!@#$%^&*]{8,}$")
    ]
    public required string Password { get; set; }

    [Required, Phone]
    public required string Phone { get; set; }

    [Timestamp]
    public DateTime CreatedAt { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? BirthDate { get; set; }
}
