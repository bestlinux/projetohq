using AutoMapper;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.HQs.Commands;
using ProjetoHQApi.Application.UseCases.HQs.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class HQMapper : Profile
    {
        public HQMapper()
        {
            CreateMap<CreateHQCommand, HQ>();
            CreateMap<HQ, GetHQResponse>();
        }
    }
}
