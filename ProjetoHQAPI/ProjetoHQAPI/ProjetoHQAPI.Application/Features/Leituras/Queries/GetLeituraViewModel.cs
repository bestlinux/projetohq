using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.Leituras.Queries
{
    public class GetLeituraViewModel
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }

        public string Capa { get; set; }

        public string DataPublicacao { get; set; }

        public int Lido { get; set; }

        public DateTime LastModified
        {
            get { return LastModified.Date; }
            set { }
        }
    }
}
