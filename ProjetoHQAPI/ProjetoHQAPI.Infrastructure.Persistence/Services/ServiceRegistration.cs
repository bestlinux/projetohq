using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repositories;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using ProjetoHQApi.Domain.Interfaces;

namespace ProjetoHQApi.Infrastructure.Persistence.Services
{
    public static class ServiceRegistration
    {
        public static void ConfigurePersistenceApp(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                     options.UseSqlServer(connectionString, b => b.MigrationsAssembly("ProjetoHQApi.WebApi")));

            #region Repositories

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IHQRepository, HQRepository>();
            services.AddScoped<IEditoraRepository, EditoraRepository>();
            services.AddScoped<IFraseRepository, FraseRepository>();
            services.AddScoped<IColecaoRepository, ColecaoRepository>();
            services.AddScoped<IDesejoRepository, DesejoRepository>();
            services.AddScoped<ILeituraRepository, LeituraRepository>();

            #endregion Repositories
        }
    }
}