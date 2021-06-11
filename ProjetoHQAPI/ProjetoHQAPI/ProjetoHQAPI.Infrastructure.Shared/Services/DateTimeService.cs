using ProjetoHQApi.Application.Interfaces;
using System;

namespace ProjetoHQApi.Infrastructure.Shared.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime NowUtc => DateTime.UtcNow;
    }
}