using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum AbsenceType
{
    [Display(Name = "Отсъствие")]
    Absence,

    [Display(Name = "Закъснение")]
    Lateness
}
