using AutoMapper;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.Colecoes.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class ColecaoMapper : Profile
    {
        public ColecaoMapper()
        {
            CreateMap<CreateColecaoCommand, Colecao>();
            CreateMap<GetColecaoQuery, Colecao>();
            CreateMap<Colecao, GetColecaoResponse>();         
        }
    }
}
