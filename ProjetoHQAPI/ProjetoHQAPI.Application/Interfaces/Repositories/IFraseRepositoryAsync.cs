using ProjetoHQApi.Application.Features.Frases.Queries;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
	public interface IFraseRepositoryAsync : IGenericRepositoryAsync<Frase>
	{
		Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedFrasesReponseAsync(GetFrasesQuery requestParameters);
	}
}
