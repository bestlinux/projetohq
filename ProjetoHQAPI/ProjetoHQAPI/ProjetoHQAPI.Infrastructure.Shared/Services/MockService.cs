using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Shared.Mock;
using System.Collections.Generic;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class MockService : IMockService
    {
        public List<HQ> SeedHQS(int rowCount)
        {
            var seedHQFaker = new HQSeedBogusConfig();
            return seedHQFaker.Generate(rowCount);
        }
    }
}