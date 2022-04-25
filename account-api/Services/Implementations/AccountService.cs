using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using account_api.DTO;
using account_api.DTO.Responses;
using account_api.Models;
using account_api.Services.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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

        public async Task<GenericResponse<AccountResponse>> Register(RegisterRequest request)
        {
            try
            {
                var userAccount = await GetUserAccount(request.Email);

                if(userAccount == null)
                {
                    var passwordData = ComputePasswordSaltAndHash(request.Password);
                    var newUserAccount = new UserAccount
                    {
                        UserId = Guid.NewGuid().ToString("N"), 
                        Email = request.Email.ToLower(), 
                        DateAdded = DateTime.UtcNow, 
                        PasswordSalt = passwordData.Keys.FirstOrDefault(),
                        PasswordHash = passwordData.Values.FirstOrDefault()
                    };


                    string jsonString = JsonSerializer.Serialize(newUserAccount);
		            var payload =  new StringContent(jsonString, Encoding.UTF8, "application/json");

                    string url = $"{_configuration.GetSection("Firebase:DatabaseUrl").Value}" +
                        $"{_configuration.GetSection("Firebase:DatabaseName").Value}/" +
                        $"{newUserAccount.UserId}.json";

                    var httpClient = _httpClientFactory.CreateClient();
                    var httpResponseMessage = await httpClient.PutAsync(url, payload);

                    if (httpResponseMessage.IsSuccessStatusCode)
                    {
                        var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();

                        var result = JsonSerializer.Deserialize<UserAccount>(contentStream);

                        return new GenericResponse<AccountResponse>
                        {
                            Data = new AccountResponse
                            {
                                UserId = result.UserId,
                                Email = result.Email
                            }, 
                            StatusCode = (int)HttpStatusCode.OK
                        };
                    }

                    return null;

                }


                return new GenericResponse<AccountResponse>
                {
                    Data = null,
                    ResponseMessage = "User already exists",
                    StatusCode = (int)HttpStatusCode.Conflict
                };
            }
            catch (System.Exception ex)
            {
                return new GenericResponse<AccountResponse>
                {
                    Data = null,
                    ResponseMessage = "Server Error",
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
        }


        private async Task<UserAccount> GetUserAccount(string email)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, 
                $"{_configuration.GetSection("Firebase:DatabaseUrl").Value}{_configuration.GetSection("Firebase:DatabaseName").Value}.json"
            );

            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if(httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStringAsync();
                
                if(contentStream != null && contentStream != "null")
                {
                    var result = JsonSerializer.Deserialize<Dictionary<string, UserAccount>>(contentStream);

                    List<UserAccount> entries = result.Select(x => x.Value).ToList();

                    return entries.FirstOrDefault(m => m.Email.Equals(email.Trim().ToLower()));

                }
                
            }

            return null;
        }


        private Dictionary<string, string> ComputePasswordSaltAndHash(string password)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            byte[] salt = new byte[128 / 8];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));


            result.Add(Convert.ToBase64String(salt), hashed);

            return result;
        }

        private bool IsPasswordCorrect(string userPassword, string savedPasswordSalt, string savedPasswordHash)
        {
            string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: userPassword,
            salt: Convert.FromBase64String(savedPasswordSalt),
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

            if (computedHash != savedPasswordHash)
            {
                return false;
            }

            return true;
                  
        }
    }
}