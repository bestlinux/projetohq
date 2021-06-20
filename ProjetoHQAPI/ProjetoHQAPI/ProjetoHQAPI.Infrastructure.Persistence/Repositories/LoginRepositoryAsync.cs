using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
	public class LoginRepositoryAsync : GenericRepositoryAsync<Usuario>, ILoginRepositoryAsync
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly DbSet<Usuario> _usuario;
		private IDataShapeHelper<Usuario> _dataShaper;
		private readonly IMockService _mockData;

		public LoginRepositoryAsync(ApplicationDbContext dbContext,
		   IDataShapeHelper<Usuario> dataShaper, IMockService mockData) : base(dbContext)
		{
			_dbContext = dbContext;
			_usuario = dbContext.Set<Usuario>();
			_dataShaper = dataShaper;
			_mockData = mockData;
		}

		public async Task<bool> IsExistsUsuarioAsync(string usuario, string senha)
		{
			//TODO: FUTURAMENTE CRIAR TABELA NO BANCO DE DADOS
			if (usuario.Equals("adm") && senha.Equals("hqs"))
				return true;
			else
				return false;
		}
	}
}
