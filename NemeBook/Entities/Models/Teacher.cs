namespace Entities.Models;

public class Teacher
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Class? MainClass { get; set; }

    public List<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
}
