namespace RealtimeMeetingAPI.Interfaces
{
    public interface ISoftDeletable // The default access level for all interface members is public
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedOnUtc { get; set; }
    }
}
