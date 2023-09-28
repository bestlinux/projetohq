using ProjetoHQApi.Application.Features.Editoras.Queries;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{

    public interface IEditoraRepositoryAsync : IGenericRepositoryAsync<Editora>
    {
        Task<bool> IsUniqueEditoraAsync(string editora);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedEditoraReponseAsync(GetEditoraQuery requestParameters);

    }
}
