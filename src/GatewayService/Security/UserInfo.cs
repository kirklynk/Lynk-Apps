namespace GatewayService.Security
{
    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public Dictionary<string, string> Claims { get; set; } = new();
    }
}
