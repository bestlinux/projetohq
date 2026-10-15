using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProjetoHQApi.Application.Shared.Behavior;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Services
{
    public static class ServiceExtensions
    {
        public static void ConfigureApplicationApp(this IServiceCollection services)
        {
            // Configuração do Serilog para console e arquivo
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("C:\\Users\\bestl\\OneDrive\\PROJETOHQATUAL\\projetohqlogs\\app-.log", rollingInterval: RollingInterval.Day) // Arquivo diário
                .CreateLogger();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

			services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())).AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddSerilog(dispose: true);
                    });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        }
    }
}
