using DMS.WebClient.Models;

namespace DMS.WebClient.Authentication
{
    public interface IAccountManagement
    {
        Task<bool> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync();
    }
}
