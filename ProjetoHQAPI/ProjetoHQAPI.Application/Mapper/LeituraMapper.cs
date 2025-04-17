using AutoMapper;
using ProjetoHQApi.Application.UseCases.Desejos.Commands;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;
using ProjetoHQApi.Application.UseCases.Leituras.Commands;
using ProjetoHQApi.Application.UseCases.Leituras.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class LeituraMapper : Profile
    {
        public LeituraMapper()
        {
            CreateMap<CreateLeituraCommand, Leitura>();
            CreateMap<Leitura, GetLeituraResponse>();
        }
    }
}
