namespace WebApp.Services.Interfaces
{
    public interface IIdentityService
    {
        string GetUsername();

        string GetUserToken();

        bool IsLoggedIn();

        Task<bool> Login(string userName, string password);

        void Logout();
    }
}
