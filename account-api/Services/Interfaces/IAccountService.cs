using account_api.DTO;
using account_api.DTO.Responses;

namespace account_api.Services.Interfaces
{
    public interface IAccountService
    {
        Task<GenericResponse<AccountResponse>> Register(RegisterRequest request);
        Task<GenericResponse<AccountResponse>> Login(LoginRequest request);
    }
}