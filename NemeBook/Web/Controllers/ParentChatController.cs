using Microsoft.AspNetCore.Mvc;
using Services.Services.Chats;
using Web.ViewModels;

namespace Web.Controllers;

public class ParentChatsController : Controller
{
    private readonly ChatService _chatService;

    public ParentChatsController(ChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IActionResult> Index(Guid parentUserId, Guid studentId, string subject, CancellationToken cancellationToken)
    {
        var teachers = await _chatService.SearchAvailableContactsAsync(parentUserId, subject, cancellationToken);

        if (!teachers.Any())
            return View(new ParentChatsViewModel
            {
                ParentUserId = parentUserId,
                StudentId = studentId,
                SelectedSubject = subject,
                Subjects = new List<string>(),
                Messages = new List<ChatMessageViewModel>()
            });

        var teacherUser = teachers.First();

        var chat = await _chatService.GetOrCreateDirectChatAsync(parentUserId, teacherUser.Id, cancellationToken);

        var messages = await _chatService.GetMessagesAsync(parentUserId, chat.Id, cancellationToken);

        var vm = new ParentChatsViewModel
        {
            ParentUserId = parentUserId,
            StudentId = studentId,
            ChatId = chat.Id,
            SelectedSubject = subject,
            Subjects = teachers.Select(t => t.FirstName + " " + t.LastName).ToList(),
            Messages = messages.Select(m => new ChatMessageViewModel
            {
                Id = m.Id,
                SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
                IsFromParent = m.SenderId == parentUserId,
                Text = m.Text,
                SentAt = m.SentAt
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Send(Guid parentUserId, Guid chatId, string message, string subject, Guid studentId, CancellationToken cancellationToken)
    {
        await _chatService.SendMessageAsync(parentUserId, chatId, message, cancellationToken);

        return RedirectToAction("Index", new { parentUserId, studentId, subject });
    }
}

