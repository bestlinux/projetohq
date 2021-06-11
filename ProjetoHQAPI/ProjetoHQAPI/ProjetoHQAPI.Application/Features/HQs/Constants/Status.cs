using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Features.HQs.Constants
{
    public static class Status
    {
        public static readonly Dictionary<string, int> StatusMap = new()
        {
            { "Edição única", 3 },
            { "Em circulação", 1 },
            { "Série completa", 4 },
            { "Título encerrado", 2 }            
        };

        public static int GetStatus(string status)
        {
            StatusMap.TryGetValue(status, out int codigo);
            return codigo;
        }
    }
}
