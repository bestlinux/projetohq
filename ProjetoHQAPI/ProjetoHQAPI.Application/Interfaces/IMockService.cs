using ProjetoHQApi.Domain.Entities;
using System.Collections.Generic;

namespace ProjetoHQApi.Application.Interfaces
{
    public interface IMockService
    {
        List<HQ> SeedHQS(int rowCount);
    }
}