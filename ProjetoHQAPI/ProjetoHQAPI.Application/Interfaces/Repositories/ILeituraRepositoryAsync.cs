using ProjetoHQApi.Application.Features.Leituras.Queries;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
    public interface ILeituraRepositoryAsync : IGenericRepositoryAsync<Leitura>
    {
        Task<bool> IsExistsTituloAndAnoInLeituraAsync(string titulo, string ano);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedLeituraReponseAsync(GetLeituraQuery requestParameters);
    }
}
