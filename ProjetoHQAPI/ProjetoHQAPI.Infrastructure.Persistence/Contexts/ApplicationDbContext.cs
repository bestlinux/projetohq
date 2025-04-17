using ProjetoHQApi.Domain.Common;
using ProjetoHQApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace ProjetoHQApi.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options
            ) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<HQ> HQs { get; set; }

        public DbSet<Desejo> Desejos { get; set; }

        public DbSet<Editora> Editoras { get; set; }

        public DbSet<Frase> Frases { get; set; }

        public DbSet<Colecao> Colecao { get; set; }

        public DbSet<Leitura> Leituras { get; set; }
       
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