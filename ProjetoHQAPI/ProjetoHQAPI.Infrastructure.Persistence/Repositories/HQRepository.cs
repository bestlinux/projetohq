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
using ProjetoHQApi.Domain.Interfaces;
using ProjetoHQApi.Application.UseCases.HQs.Queries;
using ProjetoHQApi.Application.UseCases.Desejos.Queries;

namespace ProjetoHQApi.Infrastructure.Persistence.Repositories
{
    public class HQRepository(ApplicationDbContext context) : Repository<HQ>(context), IHQRepository
    {
        public async Task<bool> IsUniqueHQTituloAsync(string hqTitulo)
        {
            return await _db.HQs
                .AllAsync(p => p.Titulo != hqTitulo);
        }

        public async Task<bool> IsExistsTituloAndAnoInHQAsync(string hqTitulo, string ano)
        {
            var data2 = await _db.HQs.Where(d => d.Titulo.ToLower().Contains(hqTitulo.ToLower()) && d.DataPublicacao.ToLower().Contains(ano)).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }

        public async Task<bool> IsExistsEditoraInHQAsync(string editora)
        {
            var data2 = await _db.HQs.Where(d => d.Editora.ToLower().Contains(editora.ToLower())).ToListAsync();

            if (data2.Count > 0)
                return true;
            else
                return false;
        }


        public async Task<IEnumerable<HQ>> GetPagedHQInWebReponseAsync(string titulo, string editora, int categoria, int genero, int status, int formato, int numeroEdicao = 0, string anoLancamento = null)
        {
           
            StringBuilder GuiaDosQuadrinhos = new();
            List<HQ> listaHq = new();
            IQueryable<HQ> result;
            int recordsTotal =0, recordsFiltered=0;
            string fields = "";
           
            GuiaDosQuadrinhos.Append("http://www.guiadosquadrinhos.com/busca-avancada-resultado.aspx?");
            GuiaDosQuadrinhos.Append("tit=");
            GuiaDosQuadrinhos.Append(titulo.Trim());
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("num=");
            GuiaDosQuadrinhos.Append(numeroEdicao != 0 ? numeroEdicao : "");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("edi=");
            GuiaDosQuadrinhos.Append(editora.Trim());
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("lic=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("art=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("per=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("cat=");
            GuiaDosQuadrinhos.Append((categoria != 0 ? categoria : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("gen=");
            GuiaDosQuadrinhos.Append((genero != 0 ? genero : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("sta=");
            GuiaDosQuadrinhos.Append((status != 0 ? status : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("for=");
            GuiaDosQuadrinhos.Append((formato != 0 ? formato : ""));
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("capa=0");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("mesi=");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("anoi=");
            GuiaDosQuadrinhos.Append(anoLancamento != null ? anoLancamento.Trim() : "");
            GuiaDosQuadrinhos.Append('&');
            GuiaDosQuadrinhos.Append("mesf=&anof=");
            GuiaDosQuadrinhos.Append(anoLancamento != null ? anoLancamento.Trim() : "");


            var doc = new HtmlAgilityPack.HtmlDocument();

            HtmlWeb web = new();

            doc = web.Load(GuiaDosQuadrinhos.ToString());

            var htmlNodes = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, 'products')]]");

            int index = 0;

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
                                        Editora = editora.Trim()
                                    };

                                    string procura = @"'MainContent_lstProfileView_div_box_msg_" + j + "'";

                                    var divNode2 = doc.DocumentNode.SelectNodes("//*[@*[contains(@id, " + procura + ")]]");

                                    var linkNodesLink = divNode2.Descendants("a");

                                    var filteredTitulo = linkNodesLink.Select(n => n.Attributes["title"].Value);

                                    foreach (var f in filteredTitulo)
                                    {
                                        StringWriter myWriter = new();
                                        HttpUtility.HtmlDecode(f.ToString(), myWriter);

                                        index = myWriter.ToString().LastIndexOf(@"-");
                                        hqEncontrada.Titulo = myWriter.ToString().Substring(0, index).Trim();
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

            return result;

        }
    }
}
