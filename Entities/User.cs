namespace BlogSharp.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Phone { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateOnly? BirthDate { get; set; }

	public bool isDeleted {get;set;} = false;

    // TODO: add blogs collection
}
