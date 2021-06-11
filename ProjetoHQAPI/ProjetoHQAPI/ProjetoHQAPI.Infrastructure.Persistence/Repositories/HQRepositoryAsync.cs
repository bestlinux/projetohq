using ProjetoHQApi.Application.Features.HQs.Queries;
using ProjetoHQApi.Application.Interfaces;
using ProjetoHQApi.Application.Interfaces.Repositories;
using ProjetoHQApi.Application.Parameters;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using ProjetoHQApi.Infrastructure.Persistence.Repository;
using HtmlAgilityPack;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class HQRepositoryAsync : GenericRepositoryAsync<HQ>, IHQRepositoryAsync
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<HQ> _hqs;
        private IDataShapeHelper<HQ> _dataShaper;
        private readonly IMockService _mockData;

        public HQRepositoryAsync(ApplicationDbContext dbContext,
            IDataShapeHelper<HQ> dataShaper, IMockService mockData) : base(dbContext)
        {
            _dbContext = dbContext;
            _hqs = dbContext.Set<HQ>();
            _dataShaper = dataShaper;
            _mockData = mockData;
        }

        public async Task<bool> IsUniqueHQTituloAsync(string hqTitulo)
        {
            return await _hqs
                .AllAsync(p => p.Titulo != hqTitulo);
        }

        public async Task<bool> IsExistsEditoraInHQAsync(string editora)
        {
            var data2 = await _hqs.Where(d => d.Editora.ToLower().Contains(editora.ToLower())).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> SeedDataAsync(int rowCount)
        {
            foreach (HQ hq in _mockData.SeedHQS(rowCount))
            {
                await this.AddAsync(hq);
            }
            return true;
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedHQReponseAsync(GetHQQuery requestParameter)
        {
            var hqTitulo = requestParameter.Titulo;
            var hqEditora = requestParameter.Editora;

            var pageNumber = requestParameter.PageNumber;
            var pageSize = requestParameter.PageSize;
            var orderBy = requestParameter.OrderBy;
            var fields = requestParameter.Fields;

            int recordsTotal, recordsFiltered;

            // Setup IQueryable
            var result = _hqs
                .AsNoTracking()
                .AsExpandable();

            // Count records total
            recordsTotal = await result.CountAsync();

            // filter data
            FilterByColumn(ref result, hqTitulo, hqEditora);

            // Count records after filter
            recordsFiltered = await result.CountAsync();

            //set Record counts
            var recordsCount = new RecordsCount
            {
                RecordsFiltered = recordsFiltered,
                RecordsTotal = recordsTotal
            };

            // set order by
            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                result = result.OrderBy(orderBy);
            }

            // select columns
            if (!string.IsNullOrWhiteSpace(fields))
            {
                result = result.Select<HQ>("new(" + fields + ")");
            }
            // paging
            result = result
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            // retrieve data to list
            var resultData = await result.ToListAsync();
            // shape data
            var shapeData = _dataShaper.ShapeData(resultData, fields);

            return (shapeData, recordsCount);
        }

        private void FilterByColumn(ref IQueryable<HQ> hqs,
            string hqTitulo,
            string hqEditora
            )
        {
            if (!hqs.Any())
                return;

            if (string.IsNullOrEmpty(hqTitulo) &&
                string.IsNullOrEmpty(hqEditora))
                return;

            var predicate = PredicateBuilder.New<HQ>();

            if (!string.IsNullOrEmpty(hqTitulo))
                predicate = predicate.Or(p => p.Titulo.Contains(hqTitulo.Trim()));

            if (!string.IsNullOrEmpty(hqEditora))
                predicate = predicate.Or(p => p.Editora.Contains(hqEditora.Trim()));

            hqs = hqs.Where(predicate);
        }

        public async Task<(IEnumerable<Entity> data, RecordsCount recordsCount)> GetPagedHQInWebReponseAsync(GetHQInWeb requestParameters)
        {
           
            StringBuilder GuiaDosQuadrinhos = new();
            List<HQ> listaHq = new();
            IQueryable<HQ> result;
            int recordsTotal =0, recordsFiltered=0;
            string fields = "";
           
            GuiaDosQuadrinhos.Append("http://www.guiadosquadrinhos.com/busca-avancada-resultado.aspx?");
            GuiaDosQuadrinhos.Append("tit=");
            GuiaDosQuadrinhos.Append(requestParameters.Titulo.Trim());
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("num=");
            GuiaDosQuadrinhos.Append(requestParameters.NumeroEdicao != 0 ? requestParameters.NumeroEdicao : "");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("edi=");
            GuiaDosQuadrinhos.Append(requestParameters.Editora.Trim());
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("lic=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("art=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("per=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("cat=");
            GuiaDosQuadrinhos.Append((requestParameters.Categoria != 0 ? requestParameters.Categoria : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("gen=");
            GuiaDosQuadrinhos.Append((requestParameters.Genero != 0 ? requestParameters.Genero : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("sta=");
            GuiaDosQuadrinhos.Append((requestParameters.Status != 0 ? requestParameters.Status : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("for=");
            GuiaDosQuadrinhos.Append((requestParameters.Formato != 0 ? requestParameters.Formato : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("capa=0");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("mesi=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("anoi=");
            GuiaDosQuadrinhos.Append(requestParameters.AnoLancamento != null ? requestParameters.AnoLancamento.Trim() : "");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("mesf=&anof=");
            GuiaDosQuadrinhos.Append(requestParameters.AnoLancamento != null ? requestParameters.AnoLancamento.Trim() : "");


            var doc = new HtmlAgilityPack.HtmlDocument();

            HtmlWeb web = new();

            doc = web.Load(GuiaDosQuadrinhos.ToString());

            var htmlNodes = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, 'products')]]");

            if (htmlNodes != null)
            {
                foreach (var node in htmlNodes)
                {
                    if (node.InnerText.Contains("Sua pesquisa não encontrou"))
                    {
                        recordsTotal = 0;
                        break;
                    }
                    else
                    {
                        string[] hosts = { "edicao" };

                        var divNode = doc.DocumentNode.SelectNodes("//*[@*[contains(@class, 'Lista_album_imagem_colecao')]]");

                        foreach (var node1 in divNode)
                        {
                            var divNode1 = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, 'MainContent_lstProfileView_div_box_msg')]]");

                            int i = divNode1.Count;
                            recordsTotal = i;

                            foreach (var node2 in divNode1)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    var hqEncontrada = new HQ
                                    {
                                        Editora = requestParameters.Editora.Trim()
                                    };

                                    string procura = @"'MainContent_lstProfileView_div_box_msg_" + j + "'";

                                    var divNode2 = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, " + procura + ")]]");

                                    var linkNodesLink = divNode2.Descendants("a");

                                    var filteredTitulo = linkNodesLink.Select(n => n.Attributes["title"].Value);

                                    foreach (var f in filteredTitulo)
                                    {
                                        StringWriter myWriter = new();
                                        HttpUtility.HtmlDecode(f.ToString(), myWriter);
                                        hqEncontrada.Titulo = myWriter.ToString();
                                        break;
                                    }

                                    var filteredLink = linkNodesLink.Select(n => n.Attributes["href"].Value);

                                    StringBuilder link = new();

                                    link.Append("http://www.guiadosquadrinhos.com/");

                                    foreach (var f in filteredLink)
                                    {
                                        link.Append(f.ToString().Replace(" ", ""));
                                        hqEncontrada.LinkDetalhes = link.ToString();
                                        break;
                                    }

                                    var linkNodesThumbs = divNode2.Descendants("img");

                                    var filteredThumb = linkNodesThumbs.Select(n => n.Attributes["src"].Value);

                                    StringBuilder thumbs = new();

                                    thumbs.Append("http://www.guiadosquadrinhos.com/");

                                    foreach (var f in filteredThumb)
                                    {
                                        thumbs.Append(f.ToString().Replace(" ", ""));
                                        hqEncontrada.ThumbCapa = thumbs.ToString();
                                        break;
                                    }

                                    listaHq.Add(hqEncontrada);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            result = listaHq.AsQueryable();

            //set Record counts
            var recordsCount = new RecordsCount
            {
                RecordsFiltered = recordsFiltered,
                RecordsTotal = recordsTotal
            };

            // retrieve data to list
            var resultData = await Task.Run(() => result.ToList());

            // shape data
            var shapeData = _dataShaper.ShapeData(resultData, fields);

            return (shapeData, recordsCount);
        }
    }
}
