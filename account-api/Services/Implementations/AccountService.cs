using System.Text.Json;
using account_api.DTO;
using account_api.DTO.Responses;
using account_api.Models;
using account_api.Services.Interfaces;

namespace account_api.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory; 
        public AccountService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public Task<GenericResponse<AccountResponse>> Login(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<GenericResponse<AccountResponse>> Register(RegisterRequest request)
        {
            try
            {
                
            }
            catch (System.Exception ex)
            {
                
                throw;
            }
        }


        public async UserAccount GetUserAccount(string email)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, 
                $"{_configuration.GetSection("Firebase:DatabaseUrl").Value}{_configuration.GetSection("Firebase:DatabaseName")}.json"
            );

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if(httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<Dictionary<string, UserAccount>>>(contentStream);

                foreach (var item in result)
                {
                    var userAccount = item.Values as UserAccount;
                }

            }

            return null;
        }
    }
}