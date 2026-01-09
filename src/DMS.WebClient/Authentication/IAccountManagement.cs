using DMS.WebClient.Models;
using Shared.Models;

namespace DMS.WebClient.Authentication
{
    public interface IAccountManagement
    {
        Task<bool> LoginAsync(LoginModel request);
        Task<bool> LogoutAsync();

        public List<Subscription> Subscriptions { get; set; }
    }
}
