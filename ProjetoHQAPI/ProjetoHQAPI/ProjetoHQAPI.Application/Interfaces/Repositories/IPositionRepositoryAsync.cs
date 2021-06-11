using ProjetoHQApi.Application.Features.Positions.Queries.GetPositions;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Interfaces.Repositories
{
    public interface IPositionRepositoryAsync : IGenericRepositoryAsync<Position>
    {
        Task<bool> IsUniquePositionNumberAsync(string positionNumber);

        Task<bool> SeedDataAsync(int rowCount);

        Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedPositionReponseAsync(GetPositionsQuery requestParameters);
    }
}