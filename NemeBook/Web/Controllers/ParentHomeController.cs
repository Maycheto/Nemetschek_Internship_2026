using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers;

public class ParentHomeController : Controller
{
    // Inject your data service (replace with your actual service)
    private readonly IParentPortalService _service;

    public ParentHomeController(IParentPortalService service)
    {
        _service = service;
    }

    public IActionResult Index(Guid? studentId)
    {
        // Load children list
        var children = _service.GetChildren();

        // Determine selected student
        var selectedId = studentId ?? children.FirstOrDefault()?.StudentId;

        var vm = new ParentHomeViewModel
        {
            Children = children,
            SelectedStudentId = selectedId,
            SelectedStudentName = _service.GetStudentName(selectedId),
            Summary = _service.GetChildSummary(selectedId),
            SubjectAverages = _service.GetSubjectAverages(selectedId),
            RecentActivity = _service.GetRecentActivity(selectedId),
            TodaySchedule = _service.GetTodaySchedule(selectedId)
        };

        return View(vm);
    }
}
