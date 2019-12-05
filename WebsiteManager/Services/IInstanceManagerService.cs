using System.Threading.Tasks;
using WebsiteManager.Models;

namespace WebsiteManager.Services
{
    public interface IInstanceManagerService
    {
        Task<bool> TryCreateNewInstance(InstanceConfiguration instanceConfiguration);

        bool TryDeleteInstance(InstanceConfiguration instanceConfiguration);
    }
}