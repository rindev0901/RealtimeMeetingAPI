namespace RealtimeMeetingAPI.Dtos
{
    public class MemberDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime LastActive { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
        public bool Locked { get; set; } = false;
        public PermissionDto Permission { get; set; } = new();
    }
}
