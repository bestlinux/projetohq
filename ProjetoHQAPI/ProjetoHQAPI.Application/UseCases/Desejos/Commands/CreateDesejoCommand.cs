using AutoMapper;
using HtmlAgilityPack;
using MediatR;
using ProjetoHQApi.Application.Constantes;
using ProjetoHQApi.Application.Services.Notifications;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Domain.Interfaces;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ProjetoHQApi.Application.UseCases.Desejos.Commands
{
    public partial class CreateDesejoCommand : IRequest<Nullable<Guid>>
    {
        public Guid? DesejoId { get; set; }
        public string Editora { get; set; }

        public string Titulo { get; set; }

        public string AnoLancamento { get; set; }

        public int NumeroEdicao { get; set; }

        public int Categoria { get; set; }

        public int Genero { get; set; }

        public int Status { get; set; }

        public int Formato { get; set; }

        public string LinkDetalhes { get; set; }

        public string ThumbCapa { get; set; }
    }

    public class CreateDesejoCommandHandler : IRequestHandler<CreateDesejoCommand, Nullable<Guid>>
    {
        private readonly IDesejoRepository _desejoRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateDesejoCommandHandler(IDesejoRepository desejoRepository, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _desejoRepository = desejoRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Nullable<Guid>> Handle(CreateDesejoCommand request, CancellationToken cancellationToken)
        {
            var desejo = _mapper.Map<Desejo>(request);
            var doc1 = new HtmlAgilityPack.HtmlDocument();
            HtmlWeb web1 = new();

            try
            {
                //BUSCA DADOS DA WEB
                doc1 = web1.Load(request.LinkDetalhes);

                var licenciadorNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'licenciador')]]");

                foreach (var node in licenciadorNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.Licenciador = myWriter.ToString();
                }

                var numeroPaginasNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'paginas')]]");

                foreach (var node in numeroPaginasNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    if (node.InnerText.ToString().Equals(""))
                        desejo.NumeroPaginas = 0;
                    else
                        desejo.NumeroPaginas = Int32.Parse(myWriter.ToString());
                }

                var categoriaNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'categoria')]]");

                foreach (var node in categoriaNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.Categoria = Categoria.GetCategoria(myWriter.ToString());
                }

                var generoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'genero')]]");

                foreach (var node in generoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.Genero = Genero.GetGenero(myWriter.ToString());
                }

                var statusNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'status')]]");

                foreach (var node in statusNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.Status = Status.GetStatus(myWriter.ToString());
                }

                var formatoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'formato')]]");

                foreach (var node in formatoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    //TODO: ARRUMAR ISSO
                    desejo.Formato = 6;
                }

                var precoNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'preco')]]");

                foreach (var node in precoNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.Preco = myWriter.ToString();
                }

                var datapubliNode = doc1.DocumentNode.SelectNodes("//*[@*[contains(@id, 'data_publi')]]");

                foreach (var node in datapubliNode)
                {
                    StringWriter myWriter = new();

                    HttpUtility.HtmlDecode(node.InnerText.ToString(), myWriter);

                    desejo.DataPublicacao = myWriter.ToString();
                }

                if (await _desejoRepository.IsExistsTituloAndAnoInDesejoAsync(request.Titulo, desejo.DataPublicacao))
                {
                    await _mediator.Publish(new ErrorNotification
                    {
                        Error = "Já existe o titulo " + request.Titulo + " com a data de publicação " + desejo.DataPublicacao
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

                string capaBancoDados = Guid.NewGuid().ToString() + ConstantesProjetoHQ.FORMATO_IMAGEM;

                WebClient client1 = new();
                Stream stream = client1.OpenRead(capaEndereco.ToString());
                Bitmap bitmap; bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(ConstantesProjetoHQ.DIRETORIO_IMAGENS_DESEJOS + capaBancoDados, ImageFormat.Jpeg);
                }

                stream.Flush();
                stream.Close();
                client1.Dispose();

                desejo.Capa = capaBancoDados;

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
                desejo.Personagens = personagens.ToString();
                desejo.DesenhosRoteirosArteFinalCores = desenhosRoteirosArteFinalCores.ToString();
                desejo.Lido = 1;
                //******************

                await _desejoRepository.AddAsync(desejo);
                await _unitOfWork.Commit(cancellationToken);

                await _mediator.Publish(new ColecaoActionNotification
                {
                    ColecaoId = request.DesejoId,
                    Action = ActionNotification.Created
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Publish(new ErrorNotification
                {
                    Error = "Ocorreu um erro ao excluir a criar o desejo de id " + request.DesejoId,
                    Stack = ex.StackTrace,
                }, cancellationToken);
                return null;
            }

            return desejo.Id;
        }
	}
}
