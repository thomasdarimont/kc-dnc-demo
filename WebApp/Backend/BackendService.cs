using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApp.Backend
{
    public class BackendService : IBackendService
    {
        private readonly HttpClient _client;
        private readonly string _backendUrl;

        public BackendService(IConfiguration configuration, HttpClient client)
        {
            _client = client;
            _backendUrl = configuration["BackendService:baseUrl"];
        }

        public async Task<string> GetUserDataAsync()
        {
            return await CallBackendServiceAsync($"{_backendUrl}/user");
        }

        public async Task<string> GetAdminDataAsync()
        {
            return await CallBackendServiceAsync($"{_backendUrl}/admin");
        }

        private async Task<string> CallBackendServiceAsync(string url)
        {
            try
            {
                return await _client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                return "Error: " + e.Message;
            }
        }
    }
}