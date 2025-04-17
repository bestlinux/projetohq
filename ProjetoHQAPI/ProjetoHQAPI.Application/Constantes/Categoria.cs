using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Constantes
{
    public static class Categoria
    {
        public static readonly Dictionary<string, int> CategoriaMap = new()
        {
            { "Álbum de Luxo", 3 },
            { "Edição Encadernada", 5 },
            { "Edição Especial", 6 },
            { "Graphic Novel", 1 },
            { "Minissérie", 2 },
            { "Revista Periódica",4 }
        };

        public static int GetCategoria(string categoria)
        {
            CategoriaMap.TryGetValue(categoria, out int codigo);
            return codigo;
        }
    }
}
