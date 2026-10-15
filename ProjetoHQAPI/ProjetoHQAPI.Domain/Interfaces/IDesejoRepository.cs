using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface IDesejoRepository : IRepository<Desejo>
    {
        Task<bool> IsExistsTituloAndAnoInDesejo(string titulo, string ano);

        Task<IEnumerable<Desejo>> LocalizaTodosDesejosPorTitulo(string titulo, CancellationToken cancellation);
    }
}
