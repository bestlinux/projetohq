using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Frases.Queries
{
    public class GetFraseResponse
    {
        public Guid Id { get; set; }
        public string NomeHQ { get; set; }

        public string Autor { get; set; }

        public string Arquivo { get; set; }
    }
}
