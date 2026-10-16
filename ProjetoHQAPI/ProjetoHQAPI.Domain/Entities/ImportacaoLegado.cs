using ProjetoHQApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetoHQApi.Domain.Entities
{
    [Table("Importacao_Legado")]
    public class ImportacaoLegado : Entity
    {
        public string Titulo { get; set; } = string.Empty;

        public string Editora { get; set; } = string.Empty;

        public int Edicao { get; set; } = 1;
    }
}
