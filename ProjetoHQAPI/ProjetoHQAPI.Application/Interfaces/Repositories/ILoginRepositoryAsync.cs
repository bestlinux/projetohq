using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
	public interface ILoginRepositoryAsync : IGenericRepositoryAsync<Usuario>
	{
		Task<bool> IsExistsUsuarioAsync(string usuario, string senha);
	}
}
