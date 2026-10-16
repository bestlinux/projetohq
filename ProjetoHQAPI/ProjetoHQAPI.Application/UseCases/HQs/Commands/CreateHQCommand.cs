using ProjetoHQApi.Domain.Entities;
using AutoMapper;
using HtmlAgilityPack;
using MediatR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.Leituras.Commands;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.Common.Converters;
using System.Text.Json.Serialization;

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public partial class CreateHQCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }
        public string? Editora { get; set; }

        public string? Titulo { get; set; }

        public string? AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public string? LinkDetalhes { get; set; }

        public string? ThumbCapa { get; set; }

        public string? Capa { get; set; }

        public string? Preco { get; set; }

        public string? Licenciador { get; set; }

        public string? Personagens { get; set; }

        public string? DesenhosRoteirosArteFinalCores { get; set; }

        public int NumeroPaginas { get; set; }

        [JsonConverter(typeof(BoolOrIntJsonConverter))]
        public int Lido { get; set; }
    }

    public class CreateHQCommandHandler : IRequestHandler<CreateHQCommand, Nullable<Guid>>
    {
        private readonly IHQRepository _hqRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly IWebScraperService _webScraperService;

        public CreateHQCommandHandler(IHQRepository hqRepository, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator, IWebScraperService webScraperService)
        {
            _hqRepository = hqRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _webScraperService = webScraperService;
        }

        public async Task<Nullable<Guid>> Handle(CreateHQCommand request, CancellationToken cancellationToken)
        {
            var hq = _mapper.Map<HQ>(request);

            if (string.IsNullOrWhiteSpace(request.LinkDetalhes))
            {
                if (string.IsNullOrWhiteSpace(hq.DataPublicacao))
                {
                    hq.DataPublicacao = request.AnoLancamento ?? DateTime.Now.Year.ToString();
                }
                if (!string.IsNullOrWhiteSpace(request.Capa))
                {
                    hq.Capa = request.Capa;
                }
                else if (string.IsNullOrWhiteSpace(hq.Capa))
                {
                    hq.Capa = request.ThumbCapa ?? "";
                }
                if (!string.IsNullOrWhiteSpace(request.Preco)) hq.Preco = request.Preco;
                if (!string.IsNullOrWhiteSpace(request.Licenciador)) hq.Licenciador = request.Licenciador;
                if (!string.IsNullOrWhiteSpace(request.Personagens)) hq.Personagens = request.Personagens;
                if (!string.IsNullOrWhiteSpace(request.DesenhosRoteirosArteFinalCores)) hq.DesenhosRoteirosArteFinalCores = request.DesenhosRoteirosArteFinalCores;
                if (request.NumeroPaginas > 0) hq.NumeroPaginas = request.NumeroPaginas;
                hq.Lido = request.Lido;

                await _hqRepository.AddAsync(hq);
                await _unitOfWork.Commit(cancellationToken);
                return hq.Id;
            }

            var doc1 = new HtmlAgilityPack.HtmlDocument();

            try
            {
                //BUSCA DADOS DA WEB
                var htmlContent = await _webScraperService.GetHtmlAsync(request.LinkDetalhes, cancellationToken);
                doc1.LoadHtml(htmlContent);

                var licenciadorNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'licenciador')]]");

                foreach (var node in licenciadorNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.Licenciador = myWriter.ToString();
                }

                var numeroPaginasNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'paginas')]]");

                foreach (var node in numeroPaginasNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    if (node.InnerText.ToString().Equals(""))
                        hq.NumeroPaginas = 0;
                    else
                        hq.NumeroPaginas = Int32.Parse(myWriter.ToString());
                }

                var categoriaNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'categoria')]]");

                foreach (var node in categoriaNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.Categoria = Categoria.GetCategoria(myWriter.ToString());
                }

                var generoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'genero')]]");

                foreach (var node in generoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.Genero = Genero.GetGenero(myWriter.ToString());
                }

                var statusNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'status')]]");

                foreach (var node in statusNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.Status = Status.GetStatus(myWriter.ToString());
                }

                var formatoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'formato')]]");

                foreach (var node in formatoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    //TODO: ARRUMAR ISSO
                    hq.Formato = 6;
                }

                var precoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'preco')]]");

                foreach (var node in precoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.Preco = myWriter.ToString();
                }

                var datapubliNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'data_publi')]]");

                foreach (var node in datapubliNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    hq.DataPublicacao = myWriter.ToString();
                }

                if (await _hqRepository.IsExistsTituloAndAnoInHQAsync(request.Titulo, hq.DataPublicacao))
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Já existe o titulo " + request.Titulo + " com a data de publicação " + hq.DataPublicacao,
                    }, cancellationToken);
                    return null;
                }


                var capaNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'cover')]]");

                var capaLink = capaNode.Descendants("a");

                var capaFilter = capaLink.Select(n => n.Attributes["href"].Value);

                string capaEndereco = null;

                foreach (var f in capaFilter)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(f.ToString(), myWriter);

                    capaEndereco = myWriter.ToString();
                }

                if (String.IsNullOrEmpty(capaEndereco))
                {
                    var capaLinkOutros = capaNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

                    foreach (var item in capaLinkOutros)
                         capaEndereco = item.ToString();

                }

                string? base64Capa = null;
                if (capaNode != null)
                {
                    var imgWithBase64 = capaNode.Descendants("img")
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x.GetAttributeValue("data-base64", null)));
                    base64Capa = imgWithBase64?.GetAttributeValue("data-base64", null);
                }

                string capaBancoDados = Guid.NewGuid().ToString() + ConstantesProjetoHQ.FORMATO_IMAGEM;
                string capaFilePath = Path.Combine(ConstantesProjetoHQ.DIRETORIO_IMAGENS, capaBancoDados);

                if (!string.IsNullOrEmpty(base64Capa) && base64Capa.Contains(","))
                {
                    try
                    {
                        var rawBase64 = base64Capa.Substring(base64Capa.IndexOf(",") + 1);
                        var imgBytes = Convert.FromBase64String(rawBase64);
                        if (!Directory.Exists(ConstantesProjetoHQ.DIRETORIO_IMAGENS))
                            Directory.CreateDirectory(ConstantesProjetoHQ.DIRETORIO_IMAGENS);

                        await File.WriteAllBytesAsync(capaFilePath, imgBytes, cancellationToken);
                        hq.Capa = capaBancoDados;
                    }
                    catch
                    {
                        // Ignore base64 save failure
                    }
                }
                else if (!string.IsNullOrEmpty(capaEndereco))
                {
                    try
                    {
                        using var client1 = new WebClient();
                        client1.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                        var imgBytes = client1.DownloadData(capaEndereco);
                        if (!Directory.Exists(ConstantesProjetoHQ.DIRETORIO_IMAGENS))
                            Directory.CreateDirectory(ConstantesProjetoHQ.DIRETORIO_IMAGENS);

                        await File.WriteAllBytesAsync(capaFilePath, imgBytes, cancellationToken);
                        hq.Capa = capaBancoDados;
                    }
                    catch
                    {
                        // Ignore external download failure so HQ import does not fail
                    }
                }

                var roteiroNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'texto_pag_detalhe')]]");

                var linkNodesLink = roteiroNode.Descendants("a");

                var filteredLink = linkNodesLink.Select(n => n.Attributes["href"]);

                StringBuilder personagens = new();
                StringBuilder desenhosRoteirosArteFinalCores = new();

                foreach (var f in filteredLink)
                {
                    StringWriter personagensWriter = new();

                    StringWriter desenhoRoteiroWriter = new();

                    if (f.Value.Contains("personagem"))
                    {
                        HttpUtility.HtmlDecode(f.OwnerNode.InnerHtml, personagensWriter);
                        personagens.Append(personagensWriter);
                        personagens.Append(';');
                    }

                    if (f.Value.Contains("artista"))
                    {
                        HttpUtility.HtmlDecode(f.OwnerNode.InnerHtml, desenhoRoteiroWriter);
                        desenhosRoteirosArteFinalCores.Append(desenhoRoteiroWriter);
                        desenhosRoteirosArteFinalCores.Append(';');
                    }
                }
                hq.Personagens = personagens.ToString();
                hq.DesenhosRoteirosArteFinalCores = desenhosRoteirosArteFinalCores.ToString();
                hq.Lido = 1;
                //******************

                await _hqRepository.AddAsync(hq);
                await _unitOfWork.Commit(cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao criar a hq com id " + request.Id,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return null;
            }

            return hq.Id;
        }
    }
}
