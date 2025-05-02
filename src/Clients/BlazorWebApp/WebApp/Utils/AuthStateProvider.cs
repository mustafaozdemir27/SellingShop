using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Extensions;

namespace WebApp.Utils
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationState _anonymous;

        public AuthStateProvider(ILocalStorageService localStorageService, HttpClient httpClient, AuthenticationState anonymous)
        {
            _localStorageService = localStorageService;
            _httpClient = httpClient;
            _anonymous = anonymous;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            String apiToken = await _localStorageService.GetToken();

            if (string.IsNullOrEmpty(apiToken))
            {
                return _anonymous;
            }

            String username = await _localStorageService.GetUsername();

            var cp = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
            }, "jwtAuthType"));

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

            return new AuthenticationState(cp);
        }

        public void NotifyUserLogin(string userName)
        {
            var cp = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName),
            }, "jwtAuthType"));
            var authState = Task.FromResult(new AuthenticationState(cp));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            var anonymous = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(anonymous);
        }
    }
}
