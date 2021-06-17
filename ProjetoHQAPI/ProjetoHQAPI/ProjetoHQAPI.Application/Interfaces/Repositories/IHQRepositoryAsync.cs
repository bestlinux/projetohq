using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
    public interface IHQRepositoryAsync : IGenericRepositoryAsync<HQ>
    {
        Task<bool> IsUniqueHQTituloAsync(string hqTitulo);

        Task<bool> SeedDataAsync(int rowCount);

        Task<bool> IsExistsEditoraInHQAsync(string editora);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedHQAdvancedSearchReponseAsync(GetHQAdvancedSearchQuery requestParameters);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedHQReponseAsync(GetHQQuery requestParameters);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedHQInWebReponseAsync(GetHQInWeb requestParameters);
    }
}
