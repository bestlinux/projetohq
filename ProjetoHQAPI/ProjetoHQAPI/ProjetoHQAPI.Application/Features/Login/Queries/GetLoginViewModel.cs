using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Login.Queries
{
	public class GetLoginViewModel
	{
		public Guid Id { get; set; }
		public string Login { get; set; }

		public string Senha { get; set; }
	}
}
