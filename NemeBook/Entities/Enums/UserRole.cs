using System.ComponentModel.DataAnnotations;

namespace Entities.Enums;

public enum UserRole
{
    [Display(Name = "Ученик")]
    Student,
    
    [Display(Name = "Родител")]
    Parent,
    
    [Display(Name = "Учител")]
    Teacher,
    
    [Display(Name = "Директор")]
    Principal
}