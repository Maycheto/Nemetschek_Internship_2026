using Entities.Enums;
using Entities.Models;

namespace Services.Dtos.Absences;

public record MarkAbsenceRequest(Guid StudentId, Guid ClassScheduleEntryId);

public record ExcuseAbsenceRequest(AbsenceExcuseReason ExcuseReason, string? ExcuseNote);

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
