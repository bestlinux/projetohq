using ProjetoHQApi.Application.DTOs.Email;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
    }
}