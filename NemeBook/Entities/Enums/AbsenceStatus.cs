using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum AbsenceStatus
{
    [Display(Name = "Неуважено")]
    Unexcused,

    [Display(Name = "Уважено")]
    Excused
}
