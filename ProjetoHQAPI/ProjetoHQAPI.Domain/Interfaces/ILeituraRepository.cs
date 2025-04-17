using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface ILeituraRepository : IRepository<Leitura>
    {
        Task<bool> IsExistsTituloAndAnoInLeituraAsync(string titulo, string ano);

        Task<IEnumerable<Leitura>> LocalizaTodasLeituras(string titulo);
    }
}
