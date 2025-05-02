using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Extensions;
using WebApp.Services.Interfaces;

namespace WebApp.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly ISyncLocalStorageService _localStorageService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        public IdentityService(HttpClient httpClient, ISyncLocalStorageService localStorageService, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public string GetUsername()
        {
            return _localStorageService.GetUsername();
        }

        public string GetUserToken()
        {
            return _localStorageService.GetToken();
        }

        public bool IsLoggedIn() => !string.IsNullOrEmpty(GetUserToken());

        public Task<bool> Login(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }
    }
}
