using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.UseCases.Editoras.Queries
{
    public class GetEditoraResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
    }
}
