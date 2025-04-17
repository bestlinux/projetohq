using AutoMapper;
using ProjetoHQApi.Application.UseCases.Colecoes.Commands;
using ProjetoHQApi.Application.UseCases.Coleções.Queries;
using ProjetoHQApi.Application.UseCases.Editoras.Commands;
using ProjetoHQApi.Application.UseCases.Editoras.Queries;
using ProjetoHQApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoHQApi.Application.Mapper
{
    public class EditoraMapper : Profile
    {
        public EditoraMapper()
        {
            CreateMap<CreateEditoraCommand, Editora>();
            CreateMap<Editora, GetEditoraResponse>();
        }
    }
}
