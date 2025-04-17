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
        Task<bool> IsExistsTituloAndAnoInDesejoAsync(string titulo, string ano);
    }
}
