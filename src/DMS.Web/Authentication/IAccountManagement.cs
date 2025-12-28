using DMS.Web.Models;

namespace DMS.Web.Authentication
{
    public interface IAccountManagement
    {
        Task<bool> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync();
    }
}
