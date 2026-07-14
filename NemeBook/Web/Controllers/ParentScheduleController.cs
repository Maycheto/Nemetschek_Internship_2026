using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers;

public class ParentScheduleController : Controller
{
    private readonly IParentPortalService _service;

    public ParentScheduleController(IParentPortalService service)
    {
        _service = service;
    }

    public IActionResult Index(Guid studentId, string day = "Mon")
    {
        var vm = new ParentScheduleViewModel
        {
            SelectedStudentId = studentId,
            SelectedDay = day,
            Entries = _service.GetSchedule(studentId, day)
        };

        return View(vm);
    }
}
