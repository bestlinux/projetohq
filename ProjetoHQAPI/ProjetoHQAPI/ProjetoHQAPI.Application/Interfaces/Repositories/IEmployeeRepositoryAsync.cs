using ProjetoHQApi.Application.Features.Employees.Queries.GetEmployees;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
    public interface IEmployeeRepositoryAsync : IGenericRepositoryAsync<Employee>
    {
        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedEmployeeReponseAsync(GetEmployeesQuery requestParameter);
    }
}