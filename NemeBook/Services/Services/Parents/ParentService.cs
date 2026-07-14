using Entities.Models;
using Services.Interfaces.Parents;
using Web.ViewModels;

namespace Services.Parents;

public class ParentService : IParentService
{
    private readonly AppDbContext _db;

    public ParentService(AppDbContext db)
    {
        _db = db;
    }

    // CRUD
    public async Task<Parent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Parents.FindAsync(new object?[] { id }, cancellationToken);

    public async Task<IReadOnlyList<Parent>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.Parents.ToListAsync(cancellationToken);

    public async Task CreateAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        _db.Parents.Add(parent);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        _db.Parents.Update(parent);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var parent = await _db.Parents.FindAsync(new object?[] { id }, cancellationToken);
        if (parent != null)
        {
            _db.Parents.Remove(parent);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    // Parent Portal Data
    public async Task<IReadOnlyList<ChildOptionViewModel>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _db.Students
            .Where(s => s.ParentId == parentId)
            .Select(s => new ChildOptionViewModel
            {
                StudentId = s.Id,
                FullName = s.FullName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ChildSummaryViewModel> GetChildSummaryAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return new ChildSummaryViewModel
        {
            FeedbacksCount = await _db.Feedbacks.CountAsync(f => f.StudentId == studentId, cancellationToken),
            AbsencesCount = await _db.Absences.CountAsync(a => a.StudentId == studentId, cancellationToken)
        };
    }

    public async Task<IReadOnlyList<SubjectAverageViewModel>> GetSubjectAveragesAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.Grades
            .Where(g => g.StudentId == studentId)
            .GroupBy(g => g.SubjectName)
            .Select(g => new SubjectAverageViewModel
            {
                SubjectName = g.Key,
                Average = g.Average(x => x.Value),
                ColorHex = "#E8590C"
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RecentActivityViewModel>> GetRecentActivityAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.Activities
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .Take(10)
            .Select(a => new RecentActivityViewModel
            {
                Description = a.Description,
                Date = a.Date
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleEntryViewModel>> GetTodayScheduleAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today.DayOfWeek.ToString().Substring(0, 3);

        return await _db.ScheduleEntries
            .Where(s => s.StudentId == studentId && s.Day == today)
            .Select(s => new ScheduleEntryViewModel
            {
                LessonNumber = s.LessonNumber,
                SubjectName = s.SubjectName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleRowViewModel>> GetScheduleAsync(Guid studentId, string day, CancellationToken cancellationToken = default)
    {
        return await _db.ScheduleEntries
            .Where(s => s.StudentId == studentId && s.Day == day)
            .Select(s => new ScheduleRowViewModel
            {
                TimeRange = s.TimeRange,
                SubjectName = s.SubjectName,
                Room = s.Room,
                TeacherName = s.TeacherName
            })
            .ToListAsync(cancellationToken);
    }

    // Chats
    public async Task<IReadOnlyList<string>> GetTeacherListAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _db.Teachers
            .Select(t => t.SubjectName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ChatMessageViewModel>> GetChatMessagesAsync(Guid studentId, string subject, CancellationToken cancellationToken = default)
    {
        return await _db.ChatMessages
            .Where(m => m.StudentId == studentId && m.Subject == subject)
            .OrderBy(m => m.Timestamp)
            .Select(m => new ChatMessageViewModel
            {
                Sender = m.Sender,
                Text = m.Text,
                Timestamp = m.Timestamp
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SendMessageAsync(Guid studentId, string subject, string message, CancellationToken cancellationToken = default)
    {
        var msg = new ChatMessage
        {
            StudentId = studentId,
            Subject = subject,
            Sender = "parent",
            Text = message,
            Timestamp = DateTime.UtcNow
        };

        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
