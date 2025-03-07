namespace RealtimeMeetingAPI.Helpers
{
    public class UserParams : PaginationParams
    {
        public string CurrentUsername { get; set; } = string.Empty;
    }
}
