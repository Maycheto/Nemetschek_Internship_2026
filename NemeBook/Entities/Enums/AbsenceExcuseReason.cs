using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum AbsenceExcuseReason
{
    [Display(Name = "Здравословни")]
    HealthReasons,

    [Display(Name = "Семейни")]
    FamilyReasons,

    [Display(Name = "Други")]
    Other
}
