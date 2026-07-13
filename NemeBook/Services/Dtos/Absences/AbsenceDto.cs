using Entities.Enums;
using Entities.Models;

namespace Services.Dtos.Absences;

public record MarkAbsenceRequest(Guid StudentId, Guid ClassScheduleEntryId);

public record ExcuseAbsenceRequest(AbsenceExcuseReason ExcuseReason, string? ExcuseNote);

/// <summary>
/// Представлява един "клас-предмет", който учителят преподава - използва се в списъка
/// "избери клас" преди да се покаже текущият час.
/// ВНИМАНИЕ: ClassName/SubjectName се попълват от навигационните пропъртита ClassSubject.Class /
/// ClassSubject.Subject - трябва да са Include-нати в IClassSubjectRepository, иначе ще са null.
/// </summary>
public record TeacherClassSubjectDto(Guid ClassSubjectId, Guid ClassId, string ClassName, Guid SubjectId, string SubjectName)
{
    public static TeacherClassSubjectDto FromEntity(ClassSubject classSubject) => new(
        classSubject.Id,
        classSubject.ClassId,
        classSubject.Class is null ? string.Empty : $"{classSubject.Class.GradeNumber}{classSubject.Class.Letter}",
        classSubject.SubjectId,
        classSubject.Subject?.Name ?? string.Empty);
}

/// <summary>
/// Текущият учебен час, намерен автоматично по програмата за избран ClassSubject.
/// </summary>
public record CurrentLessonDto(Guid ClassScheduleEntryId, Guid ClassSubjectId, int PeriodNumber, TimeOnly StartsAt, TimeOnly EndsAt)
{
    public static CurrentLessonDto FromEntity(ClassScheduleEntry entry) => new(
        entry.Id,
        entry.ClassSubjectId,
        entry.PeriodNumber,
        entry.StartsAt,
        entry.EndsAt);
}

public record AbsenceDto(
    Guid Id,
    Guid StudentId,
    Guid ClassSubjectId,
    Guid? ClassScheduleEntryId,
    DateOnly Date,
    int LessonNumber,
    AbsenceType Type,
    AbsenceStatus Status,
    AbsenceExcuseReason? ExcuseReason,
    string ExcuseNote)
{
    public static AbsenceDto FromEntity(Absence absence) => new(
        absence.Id,
        absence.StudentId,
        absence.ClassSubjectId,
        absence.ClassScheduleEntryId,
        absence.Date,
        absence.LessonNumber,
        absence.Type,
        absence.Status,
        absence.ExcuseReason,
        absence.ExcuseNote);
}