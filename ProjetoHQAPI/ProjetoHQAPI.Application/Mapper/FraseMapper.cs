using AutoMapper;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.Frases.Commands;
using ProjetoHQApi.Application.UseCases.Frases.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class FraseMapper : Profile
    {
        public FraseMapper()
        {
            CreateMap<CreateFraseCommand, Frase>();
            CreateMap<Frase, GetFraseResponse>();
        }
    }
}
