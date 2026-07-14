using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.Parents;
using Services.Interfaces.Students;
using Services.Interfaces.Grades;
using Web.ViewModels;

public class ParentHomeController : Controller
{
    private readonly IParentService parentService;
    private readonly IStudentService studentService;
    private readonly IGradeService gradeService;

    public ParentHomeController(
        IParentService parentService,
        IStudentService studentService,
        IGradeService gradeService)
    {
        this.parentService = parentService;
        this.studentService = studentService;
        this.gradeService = gradeService;
    }
    public async Task<IActionResult> Index(Guid? studentId)
    {
        var parentUserId = Guid.Parse(User.FindFirst("UserId")!.Value);

        var parent = (await parentService.GetAllAsync())
            .FirstOrDefault(p => p.UserId == parentUserId);

        if (parent == null)
            return Unauthorized();

        var children = parent.Students.ToList();

        if (!children.Any())
            return View(new ParentHomeViewModel());

        var selectedStudentId = studentId ?? children.First().Id;

        var selectedStudent = children.FirstOrDefault(s => s.Id == selectedStudentId);
        if (selectedStudent == null)
            return NotFound();   // ← добавих това, за да има return във ВСИЧКИ пътища

        var gradesVm = await gradeService.GetStudentGradesAsync(selectedStudentId);

        var averages = gradesVm.AverageBySubject
            .Select(a => new SubjectAverageViewModel
            {
                SubjectName = a.Key,
                Average = a.Value,
                ColorHex = "#E8590C"
            })
            .ToList();

        var recent = gradesVm.GradesBySubject
            .SelectMany(g => g.Value)
            .OrderByDescending(g => g.Date)
            .Take(5)
            .Select(g => new RecentActivityViewModel
            {
                Description = $"Оценка {g.Value} по {g.SubjectName}",
                Date = g.Date
            })
            .ToList();

        var todaySchedule = gradesVm.GradesBySubject
            .Select(g => new ScheduleEntryViewModel
            {
                LessonNumber = 1,
                SubjectName = g.Key
            })
            .ToList();

        var summary = new ChildSummaryViewModel
        {
            FeedbacksCount = 0,
            AbsencesCount = 0
        };

        var model = new ParentHomeViewModel
        {
            Children = children.Select(c => new ChildOptionViewModel
            {
                StudentId = c.Id,
                FullName = $"{c.User.FirstName} {c.User.LastName}"
            }).ToList(),

            SelectedStudentId = selectedStudentId,
            SelectedStudentName = $"{selectedStudent.User.FirstName} {selectedStudent.User.LastName}",

            SubjectAverages = averages,
            Summary = summary,
            TodaySchedule = todaySchedule,
            RecentActivity = recent
        };

        return View(model);   // ← гарантирано се изпълнява
    }

}