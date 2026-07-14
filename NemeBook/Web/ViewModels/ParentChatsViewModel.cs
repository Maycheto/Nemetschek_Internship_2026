namespace Web.ViewModels;

public class ParentChatsViewModel
{
    public Guid ParentUserId { get; set; }
    public Guid StudentId { get; set; }

    public Guid ChatId { get; set; }
    public string SelectedSubject { get; set; } = string.Empty;

    public List<string> Subjects { get; set; } = new();
    public List<ChatMessageViewModel> Messages { get; set; } = new();
}
