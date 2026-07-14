using Entities.Models;
using Web.ViewModels;

namespace Services.Interfaces.Parents;

public interface IParentService
{
    // Existing CRUD
    Task<Parent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Parent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(Parent parent, CancellationToken cancellationToken = default);
    Task UpdateAsync(Parent parent, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // NEW: Parent Portal Data
    Task<IReadOnlyList<ChildOptionViewModel>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    Task<ChildSummaryViewModel> GetChildSummaryAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubjectAverageViewModel>> GetSubjectAveragesAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RecentActivityViewModel>> GetRecentActivityAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduleEntryViewModel>> GetTodayScheduleAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScheduleRowViewModel>> GetScheduleAsync(Guid studentId, string day, CancellationToken cancellationToken = default);

    // NEW: Chats
    Task<IReadOnlyList<string>> GetTeacherListAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessageViewModel>> GetChatMessagesAsync(Guid studentId, string subject, CancellationToken cancellationToken = default);

    Task SendMessageAsync(Guid studentId, string subject, string message, CancellationToken cancellationToken = default);
}
