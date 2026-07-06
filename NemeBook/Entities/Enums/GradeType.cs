using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum GradeType
{
    [Display(Name = "Устно изпитване")]
    OralExamination,

    [Display(Name = "Писмено изпитване")]
    WrittenExamination,

    [Display(Name = "Практическо изпитване")]
    PracticalExamination,

    [Display(Name = "Тест")]
    Test,

    [Display(Name = "Активно участие")]
    ActiveParticipation,

    [Display(Name = "Проект")]
    Project,

    [Display(Name = "Самостоятелна работа")]
    IndividualWork,

    [Display(Name = "Домашна работа")]
    Homework,

    [Display(Name = "Контролна работа")]
    ControlWork,

    [Display(Name = "Класна работа")]
    Classwork,
    
    [Display(Name = "Входно ниво")]
    EntranceExam,
    
    [Display(Name = "Изходно ниво")]
    ExitExam
}
