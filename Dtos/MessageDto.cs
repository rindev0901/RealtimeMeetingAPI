namespace RealtimeMeetingAPI.Dtos
{
    public class MessageDto
    {
        public string SenderDisplayName { get; set; } = String.Empty;
        public string SenderUsername { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
    }
}
}
