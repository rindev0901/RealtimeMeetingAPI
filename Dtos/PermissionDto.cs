namespace RealtimeMeetingAPI.Dtos
{
    public class PermissionDto
    {
        public bool IsSelf { get; set; } = false;
        public bool HasVideo { get; set; } = false;
        public bool IsSpeaking { get; set; } = false;
        public bool IsMuted { get; set; } = true;
    }
}
