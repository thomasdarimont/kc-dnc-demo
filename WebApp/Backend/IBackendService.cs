using System.Threading.Tasks;

namespace WebApp.Backend
{
    public interface IBackendService
    {
        Task<string> GetUserDataAsync();

        Task<string> GetAdminDataAsync();
    }
}