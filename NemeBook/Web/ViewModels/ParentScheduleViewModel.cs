namespace Web.ViewModels;

public class ParentScheduleViewModel
{
    public Guid SelectedStudentId { get; set; }
    public string SelectedDay { get; set; } = "Mon";

    public Dictionary<string, string> Days { get; } = new()
    {
        { "Mon", "Mon" },
        { "Tue", "Tue" },
        { "Wed", "Wed" },
        { "Thu", "Thu" },
        { "Fri", "Fri" }
    };

    public List<ScheduleRowViewModel> Entries { get; set; } = new();
}

public class ScheduleRowViewModel
{
    public string TimeRange { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string? Room { get; set; }
    public string? TeacherName { get; set; }
}
