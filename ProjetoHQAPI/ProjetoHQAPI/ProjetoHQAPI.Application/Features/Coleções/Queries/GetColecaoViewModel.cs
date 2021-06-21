using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Colecoes.Queries
{
	public class GetColecaoViewModel
	{
		public Guid Id { get; set; }
		public string Descricao { get; set; }

		public string Arquivo { get; set; }
	}
}
