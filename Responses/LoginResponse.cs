namespace RealtimeMeetingAPI.Responses
{
    public class LoginResponse
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public DateTime? LastActive { get; set; }
    }
}
