﻿using ProjetoHQApi.Domain.Entities;
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

namespace ProjetoHQApi.Application.UseCases.HQs.Commands
{
    public partial class CreateHQCommand : IRequest<Nullable<Guid>>
    {
        public Guid Id { get; set; }
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

    public class CreateHQCommandHandler : IRequestHandler<CreateHQCommand, Nullable<Guid>>
    {
        private readonly IHQRepository _hqRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateHQCommandHandler(IHQRepository hqRepository, IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _hqRepository = hqRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Nullable<Guid>> Handle(CreateHQCommand request, CancellationToken cancellationToken)
        {
            var hq = _mapper.Map<HQ>(request);
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

                string capaBancoDados = Guid.NewGuid().ToString() + ConstantesProjetoHQ.FORMATO_IMAGEM;

                WebClient client1 = new();
                Stream stream = client1.OpenRead(capaEndereco.ToString());
                Bitmap bitmap; bitmap = new Bitmap(stream);

                if (bitmap != null)
                {
                    bitmap.Save(ConstantesProjetoHQ.DIRETORIO_IMAGENS + capaBancoDados, ImageFormat.Jpeg);
                }

                stream.Flush();
                stream.Close();
                client1.Dispose();

                hq.Capa = capaBancoDados;

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
