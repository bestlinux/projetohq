using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Frases.Queries
{
	public class GetFrasesViewModel
	{
		public Guid Id { get; set; }
		public string NomeHQ { get; set; }

		public string Arquivo { get; set; }
		public string Autor { get; set; }
	}
}
