using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoHQApi.Domain.Entities;
using ProjetoHQApi.Infrastructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ProjetoHQApi.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ImportacaoLegadoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ImportacaoLegadoController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista todos os registros pendentes na tabela Importacao_Legado
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var itens = await _context.Importacao_Legado
                .FromSqlRaw("SELECT * FROM [Importacao_Legado] WITH (NOLOCK)")
                .AsNoTracking()
                .OrderBy(x => x.Titulo)
                .ThenBy(x => x.Edicao)
                .ToListAsync(cancellationToken);

            return Ok(itens);
        }

        /// <summary>
        /// Obtém um registro específico por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var item = await _context.Importacao_Legado.FindAsync(new object[] { id }, cancellationToken);
            if (item == null) return NotFound("Registro não encontrado.");
            return Ok(item);
        }

        /// <summary>
        /// Cria um registro avulso na tabela Importacao_Legado
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ImportacaoLegadoDto dto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest("O título é obrigatório.");

            var item = new ImportacaoLegado
            {
                Id = Guid.NewGuid(),
                Titulo = dto.Titulo.Trim(),
                Editora = dto.Editora?.Trim() ?? string.Empty,
                Edicao = dto.Edicao <= 0 ? 1 : dto.Edicao,
                Created = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            await _context.Importacao_Legado.AddAsync(item, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(item);
        }

        /// <summary>
        /// Atualiza um registro existente na tabela Importacao_Legado
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ImportacaoLegadoDto dto, CancellationToken cancellationToken)
        {
            var item = await _context.Importacao_Legado.FindAsync(new object[] { id }, cancellationToken);
            if (item == null) return NotFound("Registro não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Titulo))
                item.Titulo = dto.Titulo.Trim();

            if (dto.Editora != null)
                item.Editora = dto.Editora.Trim();

            if (dto.Edicao > 0)
                item.Edicao = dto.Edicao;

            item.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Ok(item);
        }

        /// <summary>
        /// Exclui um registro da tabela Importacao_Legado
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var item = await _context.Importacao_Legado.FindAsync(new object[] { id }, cancellationToken);
            if (item == null) return NotFound("Registro não encontrado.");

            _context.Importacao_Legado.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { success = true, id });
        }

        /// <summary>
        /// Limpa todos os registros pendentes de Importacao_Legado
        /// </summary>
        [HttpDelete("limpar-tudo")]
        public async Task<IActionResult> ClearAll(CancellationToken cancellationToken)
        {
            var itens = await _context.Importacao_Legado.ToListAsync(cancellationToken);
            _context.Importacao_Legado.RemoveRange(itens);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { success = true, totalRemovido = itens.Count });
        }

        /// <summary>
        /// Exclui múltiplos registros da tabela Importacao_Legado em lote
        /// </summary>
        [HttpPost("batch-delete")]
        public async Task<IActionResult> BatchDelete([FromBody] List<Guid> ids, CancellationToken cancellationToken)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("Nenhum ID fornecido.");

            var itens = await _context.Importacao_Legado
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            _context.Importacao_Legado.RemoveRange(itens);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { success = true, totalRemovido = itens.Count });
        }

        /// <summary>
        /// Upload de arquivo .json (FormData) exportado do SQLite antigo
        /// </summary>
        [HttpPost("upload-json")]
        [RequestSizeLimit(50_000_000)] // 50MB
        public async Task<IActionResult> UploadJson([FromForm] IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhum arquivo enviado.");
            }

            using var stream = file.OpenReadStream();
            try
            {
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                return await ProcessarJsonElement(doc.RootElement, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao analisar o JSON do arquivo: {ex.Message}");
            }
        }

        /// <summary>
        /// Processa array JSON diretamente enviado no corpo da requisição
        /// </summary>
        [HttpPost("processar-json")]
        public async Task<IActionResult> ProcessarJson([FromBody] JsonElement rawJson, CancellationToken cancellationToken)
        {
            return await ProcessarJsonElement(rawJson, cancellationToken);
        }

        private async Task<IActionResult> ProcessarJsonElement(JsonElement root, CancellationToken cancellationToken)
        {
            JsonElement arrayElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                arrayElement = root;
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("hqs", out var hqProp) && hqProp.ValueKind == JsonValueKind.Array)
                    arrayElement = hqProp;
                else if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Array)
                    arrayElement = dataProp;
                else if (root.TryGetProperty("items", out var itemProp) && itemProp.ValueKind == JsonValueKind.Array)
                    arrayElement = itemProp;
                else
                {
                    var firstArray = root.EnumerateObject().FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
                    if (firstArray.Value.ValueKind == JsonValueKind.Array)
                        arrayElement = firstArray.Value;
                    else
                        return BadRequest("O JSON não contém uma lista de quadrinhos.");
                }
            }
            else
            {
                return BadRequest("Formato JSON inválido. Esperado um array de quadrinhos.");
            }

            // Carrega em memória apenas as 3 colunas necessárias via EF Core LINQ para validação ultra-rápida
            var hqsData = await _context.HQs
                .AsNoTracking()
                .Select(h => new
                {
                    Titulo = h.Titulo,
                    Editora = h.Editora,
                    NumeroEdicao = h.NumeroEdicao
                })
                .ToListAsync(cancellationToken);

            var setHqsExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in hqsData)
            {
                var t = (h.Titulo ?? string.Empty).Trim().ToLower();
                var e = (h.Editora ?? string.Empty).Trim().ToLower();
                var n = h.NumeroEdicao <= 0 ? 1 : h.NumeroEdicao;
                setHqsExistentes.Add($"{t}|{e}|{n}");
            }

            var legadoData = await _context.Importacao_Legado
                .AsNoTracking()
                .Select(l => new
                {
                    Titulo = l.Titulo,
                    Editora = l.Editora,
                    Edicao = l.Edicao
                })
                .ToListAsync(cancellationToken);

            var setLegadoExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in legadoData)
            {
                var t = (l.Titulo ?? string.Empty).Trim().ToLower();
                var e = (l.Editora ?? string.Empty).Trim().ToLower();
                var n = l.Edicao <= 0 ? 1 : l.Edicao;
                setLegadoExistentes.Add($"{t}|{e}|{n}");
            }

            int totalLidos = 0;
            int jaCadastrados = 0;
            int duplicadosIgnorados = 0;
            int novosImportados = 0;

            var novosRegistros = new List<ImportacaoLegado>();

            foreach (var elem in arrayElement.EnumerateArray())
            {
                totalLidos++;

                string tituloRaw = GetStringValue(elem, "titulo", "Titulo", "title", "Title", "nome", "Nome");
                string editoraRaw = GetStringValue(elem, "editora", "Editora", "publisher", "Publisher");
                string edicaoRaw = GetStringValue(elem, "edicao", "Edicao", "numeroEdicao", "NumeroEdicao", "numero", "Numero", "edition", "vol");

                string titulo = tituloRaw.Trim();
                string editora = editoraRaw.Trim();

                if (string.IsNullOrWhiteSpace(titulo))
                    continue;

                // Regra do usuário:
                // "edicao": "Vol. 1" (Remova qualquer caracter string que tenha além do número. Caso o campo não contenha nenhum número, pode manter o número '1')
                int edicaoNumero = ParseEdicao(edicaoRaw);

                string chaveValidacao = $"{titulo.ToLower()}|{editora.ToLower()}|{edicaoNumero}";

                // 1. Verifica se já está no SQL Server
                if (setHqsExistentes.Contains(chaveValidacao))
                {
                    jaCadastrados++;
                    continue;
                }

                // 2. Verifica se já está na fila de Importacao_Legado
                if (setLegadoExistentes.Contains(chaveValidacao))
                {
                    duplicadosIgnorados++;
                    continue;
                }

                // 3. Caso não seja encontrada, insere na tabela Importacao_Legado
                var novoItem = new ImportacaoLegado
                {
                    Id = Guid.NewGuid(),
                    Titulo = titulo,
                    Editora = editora,
                    Edicao = edicaoNumero,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                };

                novosRegistros.Add(novoItem);
                setLegadoExistentes.Add(chaveValidacao);
                novosImportados++;
            }

            if (novosRegistros.Count > 0)
            {
                await _context.Importacao_Legado.AddRangeAsync(novosRegistros, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Ok(new
            {
                success = true,
                totalLidos,
                jaCadastrados,
                novosImportados,
                duplicadosIgnorados,
                mensagem = $"Processamento concluído: {totalLidos} analisadas. {jaCadastrados} já existiam na coleção. {novosImportados} novas HQs adicionadas à fila de importação legado."
            });
        }

        private static string GetStringValue(JsonElement element, params string[] propertyNames)
        {
            foreach (var prop in propertyNames)
            {
                if (element.TryGetProperty(prop, out var val))
                {
                    if (val.ValueKind == JsonValueKind.String)
                        return val.GetString() ?? string.Empty;
                    if (val.ValueKind == JsonValueKind.Number)
                        return val.ToString();
                }
            }
            return string.Empty;
        }

        private static int ParseEdicao(string? edicaoRaw)
        {
            if (string.IsNullOrWhiteSpace(edicaoRaw))
                return 1;

            var match = Regex.Match(edicaoRaw, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int num))
                return num;

            return 1;
        }
    }

    public class ImportacaoLegadoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string? Editora { get; set; }
        public int Edicao { get; set; } = 1;
    }
}
