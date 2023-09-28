FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app/ProjetoHQAPI

# copy csproj and restore as distinct layers
COPY *.sln . 
COPY ProjetoHQAPI.WebApi/*.csproj ./ProjetoHQAPI.WebApi/
COPY ProjetoHQAPI.Application/*.csproj ./ProjetoHQAPI.Application/
COPY ProjetoHQAPI.Domain/*.csproj ./ProjetoHQAPI.Domain/
COPY ProjetoHQAPI.Infrastructure.Persistence/*.csproj ./ProjetoHQAPI.Infrastructure.Persistence/
COPY ProjetoHQAPI.Infrastructure.Shared/*.csproj ./ProjetoHQAPI.Infrastructure.Shared/

RUN dotnet restore

# copy everything else and build app
COPY ProjetoHQAPI.WebApi/. ./ProjetoHQAPI.WebApi/
COPY ProjetoHQAPI.Application/. ./ProjetoHQAPI.Application/
COPY ProjetoHQAPI.Domain/. ./ProjetoHQAPI.Domain/
COPY ProjetoHQAPI.Infrastructure.Persistence/. ./ProjetoHQAPI.Infrastructure.Persistence/
COPY ProjetoHQAPI.Infrastructure.Shared/. ./ProjetoHQAPI.Infrastructure.Shared/

WORKDIR /app/ProjetoHQAPI
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /app

COPY --from=build /app/ProjetoHQAPI/out ./
ENTRYPOINT ["dotnet", "ProjetoHQApi.WebApi.dll"]