using ProjetoHQApi.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Entities
{
	public class Colecao : Entity
    {
		public Guid Id { get; set; }
		public string Descricao { get; set; }

		public string Arquivo { get; set; }
	}
}
