using ProjetoHQApi.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Domain.Entities
{
	public class Leitura : AuditableBaseEntity
    {
        public string Titulo { get; set; }

        public string Capa { get; set; }

        public int Lido { get; set; }

        public string DataPublicacao { get; set; }

    }
}
