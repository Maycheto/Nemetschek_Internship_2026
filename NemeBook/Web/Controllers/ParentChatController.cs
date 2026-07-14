using Microsoft.AspNetCore.Mvc;
using Services.Interfaces.Chats;
using Web.ViewModels;

public class ParentChatsController : Controller
{
    private readonly IChatService chatService;

    public ParentChatsController(IChatService chatService)
    {
        this.chatService = chatService;
    }

    public async Task<IActionResult> Index(Guid? chatId)
    {
        var parentUserId = Guid.Parse(User.FindFirst("UserId")!.Value);

        var chats = await chatService.GetChatsForUserAsync(parentUserId);

        if (!chats.Any())
            return View(new ParentChatViewModel());

        var selectedChatId = chatId ?? chats.First().Id;

        var messages = await chatService.GetMessagesAsync(parentUserId, selectedChatId);

        var model = new ParentChatViewModel
        {
            Chats = chats.ToList(),
            SelectedChatId = selectedChatId,
            Messages = messages.ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Send(Guid chatId, string text)
    {
        var parentUserId = Guid.Parse(User.FindFirst("UserId")!.Value);

        await chatService.SendMessageAsync(parentUserId, chatId, text);

        return RedirectToAction("Index", new { chatId });
    }
}
