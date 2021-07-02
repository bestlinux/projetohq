using ProjetoHQApi.Application.Features.Desejos.Queries;
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
    public interface IDesejoRepositoryAsync : IGenericRepositoryAsync<Desejo>
    {
        Task<bool> IsExistsTituloAndAnoInDesejoAsync(string titulo, string ano);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedDesejoReponseAsync(GetDesejoQuery requestParameters);
    }
}
