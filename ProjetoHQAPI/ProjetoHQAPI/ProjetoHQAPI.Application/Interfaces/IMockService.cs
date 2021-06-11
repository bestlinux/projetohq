using ProjetoHQApi.Domain.Entities;
using System.Collections.Generic;

namespace ProjetoHQApi.Application.Interfaces
{
    public interface IMockService
    {
        List<Position> GetPositions(int rowCount);

        List<Employee> GetEmployees(int rowCount);

        List<Position> SeedPositions(int rowCount);

        List<HQ> SeedHQS(int rowCount);
    }
}