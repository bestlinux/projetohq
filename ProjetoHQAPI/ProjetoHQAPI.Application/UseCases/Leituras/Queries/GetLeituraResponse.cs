using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Leituras.Queries
{
    public class GetLeituraResponse
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }

        public string Capa { get; set; }

        public int Lido { get; set; }

        public string DataPublicacao { get; set; }

        public DateTime? DataLeitura { get; set; }
    }
}
