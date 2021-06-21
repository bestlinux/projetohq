using ProjetoHQApi.Application.Behaviours;
using ProjetoHQApi.Application.Helpers;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ProjetoHQApi.Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            services.AddScoped<IDataShapeHelper<HQ>, DataShapeHelper<HQ>>();
            services.AddScoped<IDataShapeHelper<Editora>, DataShapeHelper<Editora>>();
            services.AddScoped<IDataShapeHelper<Frase>, DataShapeHelper<Frase>>();
            services.AddScoped<IDataShapeHelper<Usuario>, DataShapeHelper<Usuario>>();
            services.AddScoped<IDataShapeHelper<Colecao>, DataShapeHelper<Colecao>>();


            services.AddScoped<IModelHelper, ModelHelper>();
            //services.AddScoped<IMockData, MockData>();
        }
    }
}