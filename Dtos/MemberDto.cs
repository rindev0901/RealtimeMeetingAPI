namespace RealtimeMeetingAPI.Dtos
{
    public class MemberDto
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime LastActive { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public bool Locked { get; set; }
    }
}
