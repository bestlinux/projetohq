using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Constants
{
    public static class Genero
    {
        public static readonly Dictionary<string, int> GeneroMap = new()
        {
            { "Adaptação", 25 },
            { "Alternativo", 6 },
            { "Aventura", 17 },
            { "Clássico", 5 },
            { "Drama", 12 },
            { "Educacional", 23 },
            { "Erótico", 8 },
            { "Espada e Magia", 20 },
            { "Europeu", 7 },
            { "Fantasia", 9 },
            { "Fanzine", 19 },
            { "Faroeste", 11 },
            { "Ficcão Científica", 10 },
            { "Guerra", 16 },
            { "Humor", 3 },
            { "Infantil", 2 },
            { "Institucional", 21 },
            { "Juvenil", 22 },
            { "Mangá", 4 },
            { "Policial", 14 },
            { "Revista de Informação", 18 },            
            { "Romance", 24 },
            { "Super-heróis", 1 },
            { "Suspense", 15 },
            { "Terror", 13 }            
        };

        public static int GetGenero(string genero)
        {
            try
            {
                GeneroMap.TryGetValue(genero, out int codigo);
                return codigo;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
