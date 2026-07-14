using Entities.Models;

namespace Web.ViewModels;

public class ParentChatViewModel
{
    public Guid? SelectedChatId { get; set; }

    public List<Chat> Chats { get; set; } = new();

    public List<Message> Messages { get; set; } = new();
}
