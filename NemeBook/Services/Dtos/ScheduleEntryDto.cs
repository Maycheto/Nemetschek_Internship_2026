public class ScheduleEntryDto
{
    public int LessonNumber { get; set; }
    public string SubjectName { get; set; } = "";
    public string Room { get; set; } = "";
    public string TeacherName { get; set; } = "";
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}

