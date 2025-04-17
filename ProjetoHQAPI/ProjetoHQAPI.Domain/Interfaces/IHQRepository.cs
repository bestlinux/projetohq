using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Interfaces
{
    public interface IHQRepository : IRepository<HQ>
    {
        Task<bool> IsUniqueHQTituloAsync(string hqTitulo);

        Task<bool> IsExistsEditoraInHQAsync(string editora);

        Task<bool> IsExistsTituloAndAnoInHQAsync(string titulo, string ano);

        Task<IEnumerable<HQ>> GetPagedHQInWebReponseAsync(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null);
    }
}
