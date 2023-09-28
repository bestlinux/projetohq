using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Entities
{

	public class Usuario
	{
		public Guid Id { get; set; }
		public string Login { get; set; }

		public string Senha { get; set; }

	}
}
