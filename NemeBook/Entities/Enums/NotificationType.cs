using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum NotificationType
{
    [Display(Name = "Събитие")]
    Event,

    [Display(Name = "Оценка")]
    Grade,

    [Display(Name = "Отсъствие")]
    Absence,

    [Display(Name = "Отзив")]
    Feedback,

    [Display(Name = "Съобщение")]
    Message
}
