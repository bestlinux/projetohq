﻿using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Shared.Mock;
using System.Collections.Generic;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class MockService : IMockService
    {
        public List<Position> GetPositions(int rowCount)
        {
            var positionFaker = new PositionInsertBogusConfig();
            return positionFaker.Generate(rowCount);
        }

        public List<Employee> GetEmployees(int rowCount)
        {
            var positionFaker = new EmployeeBogusConfig();
            return positionFaker.Generate(rowCount);
        }

        public List<Position> SeedPositions(int rowCount)
        {
            var seedPositionFaker = new PositionSeedBogusConfig();
            return seedPositionFaker.Generate(rowCount);
        }

        public List<HQ> SeedHQS(int rowCount)
        {
            var seedHQFaker = new HQSeedBogusConfig();
            return seedHQFaker.Generate(rowCount);
        }
    }
}