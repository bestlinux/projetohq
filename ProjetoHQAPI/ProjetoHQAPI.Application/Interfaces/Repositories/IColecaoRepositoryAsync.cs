using ProjetoHQApi.Application.Features.Colecoes.Queries;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
	public interface IColecaoRepositoryAsync : IGenericRepositoryAsync<Colecao>
	{
		Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedColecaoReponseAsync(GetColecaoQuery requestParameters);
	}
}
