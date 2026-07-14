namespace Web.ViewModels;

public class ChatMessageViewModel
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public bool IsFromParent { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}
