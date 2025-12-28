namespace DMS.Web.Models
{
    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public List<Subscription> Subscriptions { get; set; } = new();

    }
}
