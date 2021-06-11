using ProjetoHQApi.Application.Features.Editoras.Commands;
using ProjetoHQApi.Application.Features.Editoras.Queries;
using ProjetoHQApi.Application.Features.Employees.Queries.GetEmployees;
using ProjetoHQApi.Application.Features.HQs.Commands;
using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.Application.Features.Positions.Commands.CreatePosition;
using ProjetoHQApi.Application.Features.Positions.Queries.GetPositions;
using ProjetoHQApi.Domain.Entities;
using AutoMapper;

namespace ProjetoHQApi.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<Position, GetPositionsViewModel>().ReverseMap();
            CreateMap<Employee, GetEmployeesViewModel>().ReverseMap();
            CreateMap<HQ, GetHQViewModel>().ReverseMap();
            CreateMap<Editora, GetEditoraViewModel>().ReverseMap();

            CreateMap<CreatePositionCommand, Position>();
            CreateMap<CreateHQCommand, HQ>();
            CreateMap<CreateEditoraCommand, Editora>();
        }
    }
}