using Data;
using Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels;

namespace Web.Controllers;

[Authorize(Roles = "Principal")]
public class ClassManagementController : Controller
{
    private readonly NemeBookDbContext dbContext;

    public ClassManagementController(NemeBookDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Students(Guid classId, CancellationToken cancellationToken = default)
    {
        var viewModel = await BuildClassManagementViewModelAsync(
            classId,
            "Class",
            "Клас",
            cancellationToken);

        if (viewModel is null)
        {
            return NotFound();
        }

        var studentRows = await dbContext.Students
            .AsNoTracking()
            .Where(student => student.ClassId == classId && student.User.IsActive)
            .OrderBy(student => student.User.FirstName)
            .ThenBy(student => student.User.MiddleName)
            .ThenBy(student => student.User.LastName)
            .Select(student => new
            {
                student.Id,
                student.User.FirstName,
                student.User.MiddleName,
                student.User.LastName,
                AverageGrade = student.Grades
                    .Select(grade => (decimal?)grade.Value)
                    .Average(),
                PraiseCount = student.Feedbacks.Count(feedback => feedback.Type == FeedbackType.Praise),
                RemarkCount = student.Feedbacks.Count(feedback => feedback.Type == FeedbackType.Remark),
                AbsenceAndLatenessCount = student.Absences.Count(),
            })
            .ToListAsync(cancellationToken);

        viewModel.Students = studentRows
            .Select((student, index) => new PrincipalClassStudentViewModel
            {
                StudentId = student.Id,
                ClassNumber = index + 1,
                FullName = FormatFullName(student.FirstName, student.MiddleName, student.LastName),
                AverageGrade = student.AverageGrade.HasValue
                    ? Math.Round(student.AverageGrade.Value, 2)
                    : null,
                PraiseCount = student.PraiseCount,
                RemarkCount = student.RemarkCount,
                AbsenceAndLatenessCount = student.AbsenceAndLatenessCount,
            })
            .ToList();

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> SearchStudentMatches(string? query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : query.Trim();

        if (normalizedQuery.Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var studentRows = await dbContext.Students
            .AsNoTracking()
            .Where(student =>
                student.User.IsActive &&
                (student.User.FirstName.Contains(normalizedQuery) ||
                 (student.User.MiddleName != null && student.User.MiddleName.Contains(normalizedQuery)) ||
                 student.User.LastName.Contains(normalizedQuery)))
            .OrderBy(student => student.User.FirstName)
            .ThenBy(student => student.User.MiddleName)
            .ThenBy(student => student.User.LastName)
            .Select(student => new
            {
                student.ClassId,
                student.User.FirstName,
                student.User.MiddleName,
                student.User.LastName,
                student.Class.GradeNumber,
                student.Class.Letter,
            })
            .Take(20)
            .ToListAsync(cancellationToken);

        var results = studentRows.Select(student => new
        {
            fullName = FormatFullName(student.FirstName, student.MiddleName, student.LastName),
            className = $"{student.GradeNumber}{student.Letter}",
            url = Url.Action(nameof(Students), new { classId = student.ClassId }),
        });

        return Json(results);
    }

    [HttpGet]
    public async Task<IActionResult> SearchTeacherMatches(string? query, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : query.Trim();

        if (normalizedQuery.Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var teacherRows = await dbContext.Teachers
            .AsNoTracking()
            .Where(teacher =>
                teacher.User.IsActive &&
                (teacher.User.FirstName.Contains(normalizedQuery) ||
                 (teacher.User.MiddleName != null && teacher.User.MiddleName.Contains(normalizedQuery)) ||
                 teacher.User.LastName.Contains(normalizedQuery)))
            .OrderBy(teacher => teacher.User.FirstName)
            .ThenBy(teacher => teacher.User.MiddleName)
            .ThenBy(teacher => teacher.User.LastName)
            .Select(teacher => new
            {
                teacher.Id,
                teacher.User.FirstName,
                teacher.User.MiddleName,
                teacher.User.LastName,
            })
            .Take(20)
            .ToListAsync(cancellationToken);

        var results = teacherRows.Select(teacher => new
        {
            id = teacher.Id,
            fullName = FormatFullName(teacher.FirstName, teacher.MiddleName, teacher.LastName),
        });

        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMainTeacher(
        Guid classId,
        Guid? teacherId,
        CancellationToken cancellationToken = default)
    {
        var schoolClass = await dbContext.Classes
            .FirstOrDefaultAsync(currentClass => currentClass.Id == classId, cancellationToken);

        if (schoolClass is null)
        {
            return NotFound();
        }

        if (teacherId.HasValue)
        {
            var teacherExists = await dbContext.Teachers
                .AnyAsync(
                    teacher =>
                        teacher.Id == teacherId.Value &&
                        teacher.User.IsActive,
                    cancellationToken);

            if (!teacherExists)
            {
                return RedirectToAction(nameof(Students), new { classId });
            }

            var otherAssignedClasses = await dbContext.Classes
                .Where(currentClass =>
                    currentClass.Id != classId &&
                    currentClass.MainTeacherId == teacherId.Value)
                .ToListAsync(cancellationToken);

            foreach (var assignedClass in otherAssignedClasses)
            {
                assignedClass.MainTeacherId = null;
            }
        }

        schoolClass.MainTeacherId = teacherId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Students), new { classId });
    }

    [HttpGet]
    public async Task<IActionResult> Subjects(Guid classId, CancellationToken cancellationToken = default)
    {
        return await PlaceholderAsync(classId, "Subjects", "Предмети", cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Schedule(Guid classId, CancellationToken cancellationToken = default)
    {
        return await PlaceholderAsync(classId, "Schedule", "Програма", cancellationToken);
    }

    [HttpGet]
    public async Task<IActionResult> Events(Guid classId, CancellationToken cancellationToken = default)
    {
        return await PlaceholderAsync(classId, "Events", "Събития", cancellationToken);
    }

    private async Task<IActionResult> PlaceholderAsync(
        Guid classId,
        string activeTab,
        string sectionTitle,
        CancellationToken cancellationToken)
    {
        var viewModel = await BuildClassManagementViewModelAsync(
            classId,
            activeTab,
            sectionTitle,
            cancellationToken);

        if (viewModel is null)
        {
            return NotFound();
        }

        viewModel.EmptyMessage = "Тази секция ще бъде добавена по-късно.";
        return View("Placeholder", viewModel);
    }

    private async Task<PrincipalClassManagementViewModel?> BuildClassManagementViewModelAsync(
        Guid classId,
        string activeTab,
        string sectionTitle,
        CancellationToken cancellationToken)
    {
        var schoolClass = await dbContext.Classes
            .AsNoTracking()
            .Where(currentClass => currentClass.Id == classId)
            .Select(currentClass => new
            {
                currentClass.Id,
                currentClass.GradeNumber,
                currentClass.Letter,
                currentClass.MainTeacherId,
                MainTeacherFirstName = currentClass.MainTeacher == null ? null : currentClass.MainTeacher.User.FirstName,
                MainTeacherMiddleName = currentClass.MainTeacher == null ? null : currentClass.MainTeacher.User.MiddleName,
                MainTeacherLastName = currentClass.MainTeacher == null ? null : currentClass.MainTeacher.User.LastName,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (schoolClass is null)
        {
            return null;
        }

        return new PrincipalClassManagementViewModel
        {
            ClassId = schoolClass.Id,
            ClassName = $"{schoolClass.GradeNumber}{schoolClass.Letter}",
            ActiveTab = activeTab,
            SectionTitle = sectionTitle,
            MainTeacherId = schoolClass.MainTeacherId,
            MainTeacherName = schoolClass.MainTeacherId.HasValue
                ? FormatFullName(
                    schoolClass.MainTeacherFirstName ?? string.Empty,
                    schoolClass.MainTeacherMiddleName,
                    schoolClass.MainTeacherLastName ?? string.Empty)
                : null,
        };
    }

    private static string FormatFullName(string firstName, string? middleName, string lastName)
    {
        return string.Join(
            " ",
            new[] { firstName, middleName, lastName }
                .Where(name => !string.IsNullOrWhiteSpace(name)));
    }
}
