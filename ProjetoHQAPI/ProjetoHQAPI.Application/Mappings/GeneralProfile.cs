using ProjetoHQApi.Application.Features.Editoras.Commands;
using ProjetoHQApi.Application.Features.Editoras.Queries;
using ProjetoHQApi.Application.Features.HQs.Commands;
using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using ProjetoHQApi.Application.Features.Frases.Commands;
using ProjetoHQApi.Application.Features.Frases.Queries;
using ProjetoHQApi.Application.Features.Login.Queries;
using ProjetoHQApi.Application.Features.Colecoes.Queries;
using ProjetoHQApi.Application.Features.Colecoes.Commands;
using ProjetoHQApi.Application.Features.Desejos.Commands;
using ProjetoHQApi.Application.Features.Leituras.Queries;
using ProjetoHQApi.Application.Features.Leituras.Commands;

namespace ProjetoHQApi.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<HQ, GetDesejoViewModel>().ReverseMap();
            CreateMap<Editora, GetEditoraViewModel>().ReverseMap();
            CreateMap<Frase, GetFrasesViewModel>().ReverseMap();
            CreateMap<Usuario, GetLoginViewModel>().ReverseMap();
            CreateMap<Colecao, GetColecaoViewModel>().ReverseMap();
            CreateMap<Desejo, GetDesejoViewModel>().ReverseMap();
            CreateMap<Leitura, GetLeituraViewModel>().ReverseMap();

            CreateMap<CreateHQCommand, HQ>();
            CreateMap<CreateEditoraCommand, Editora>();
            CreateMap<CreateFraseCommand, Frase>();
            CreateMap<CreateColecaoCommand, Colecao>();
            CreateMap<CreateDesejoCommand, Desejo>();
            CreateMap<CreateLeituraCommand, Leitura>();
        }
    }
}