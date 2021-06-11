﻿using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IDateTimeService _dateTime;
        private readonly ILoggerFactory _loggerFactory;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IDateTimeService dateTime,
            ILoggerFactory loggerFactory
            ) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _loggerFactory = loggerFactory;
        }

        public DbSet<Position> Positions { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<HQ> HQs { get; set; }

        public DbSet<Editora> Editoras { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = _dateTime.NowUtc;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModified = _dateTime.NowUtc;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var _mockData = this.Database.GetService<IMockService>();
            var seedPositions = _mockData.SeedPositions(100000);
            var seedEmploys = _mockData.GetEmployees(100000);
            var seedHQs = _mockData.SeedHQS(10);

            builder.Entity<Position>().HasData(seedPositions);
            builder.Entity<Employee>().HasData(seedEmploys);
            builder.Entity<HQ>().HasData(seedHQs);

            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }
    }
}