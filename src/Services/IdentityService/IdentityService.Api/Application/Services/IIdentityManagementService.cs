using IdentityService.Api.Application.Models;

namespace IdentityService.Api.Application.Services
{
    public interface IIdentityManagementService
    {
        Task<LoginResponseModel> Login(LoginRequestModel model);
    }
}
