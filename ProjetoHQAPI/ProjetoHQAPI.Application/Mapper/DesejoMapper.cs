using AutoMapper;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.Desejos.Commands;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class DesejoMapper : Profile
    {
        public DesejoMapper()
        {
            CreateMap<CreateDesejoCommand, Desejo>();
            CreateMap<GetDesejoQuery, Desejo>();
            CreateMap<Desejo, GetDesejoResponse>();
        }
    }
}
