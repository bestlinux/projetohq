using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface IWebScraperService
    {
        Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default);
    }
}
