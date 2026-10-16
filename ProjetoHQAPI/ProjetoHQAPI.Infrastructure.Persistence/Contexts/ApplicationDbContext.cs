using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime
            ) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
        }

        public DbSet<HQ> HQs { get; set; }

        public DbSet<Desejo> Desejos { get; set; }

        public DbSet<Editora> Editoras { get; set; }

        public DbSet<Frase> Frases { get; set; }

        public DbSet<Colecao> Colecao { get; set; }

        public DbSet<Leitura> Leituras { get; set; }

        public DbSet<ImportacaoLegado> Importacao_Legado { get; set; }
        public DbSet<LogUsoIA> Log_Uso_IA { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = _dateTime.NowUtc;
                        entry.Entity.LastModified = _dateTime.NowUtc;
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
            base.OnModelCreating(builder);

            // aplica as configurações de mapeamento das entidades
            // do banco de dados contidas em uma determinada assembly
            // (conjunto de classes) ao objeto ModelBuilder durante a
            // criação do modelo.
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}