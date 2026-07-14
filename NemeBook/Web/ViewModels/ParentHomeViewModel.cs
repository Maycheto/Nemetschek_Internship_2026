namespace Web.ViewModels;

public class ParentHomeViewModel
{
    public List<ChildOptionViewModel> Children { get; set; } = new();

    public Guid? SelectedStudentId { get; set; }
    public string? SelectedStudentName { get; set; }

    public ChildSummaryViewModel? Summary { get; set; }

    public List<SubjectAverageViewModel> SubjectAverages { get; set; } = new();
    public List<RecentActivityViewModel> RecentActivity { get; set; } = new();
    public List<ScheduleEntryViewModel> TodaySchedule { get; set; } = new();
}

public class ChildOptionViewModel
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class ChildSummaryViewModel
{
    public int FeedbacksCount { get; set; }
    public int AbsencesCount { get; set; }
}

public class SubjectAverageViewModel
{
    public string SubjectName { get; set; } = string.Empty;
    public decimal Average { get; set; }
    public string ColorHex { get; set; } = "#E8590C";
}

public class RecentActivityViewModel
{
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class ScheduleEntryViewModel
{
    public int LessonNumber { get; set; }
    public string SubjectName { get; set; } = string.Empty;
}
