import { HQ, JevColecaoMatch } from '../types';

/**
 * Motor Híbrido de Decisão Local (JEV Engine) - Versão Web (Sistema 1 Local)
 * Executa deduplicação probabilística, normalização canônica e matching multi-critério
 * com latência de 0ms diretamente no navegador.
 */

const EDITORAS_CANONICAS: Record<string, string> = {
  'panini comics': 'Panini',
  'panini brasil': 'Panini',
  'editora panini': 'Panini',
  'panini books': 'Panini',
  'panini': 'Panini',
  'editora abril': 'Abril',
  'abril jovem': 'Abril',
  'abril': 'Abril',
  'dc comics': 'DC',
  'dc': 'DC',
  'marvel comics': 'Marvel',
  'marvel': 'Marvel',
  'mythos editora': 'Mythos',
  'mythos': 'Mythos',
  'editora globo': 'Globo',
  'globo': 'Globo',
  'pipoca & nanquim': 'Pipoca & Nanquim',
  'pipoca e nanquim': 'Pipoca & Nanquim',
  'pipoca nanquim': 'Pipoca & Nanquim',
  'darkside books': 'Darkside',
  'darkside': 'Darkside',
  'editora jbc': 'JBC',
  'jbc': 'JBC',
  'editora veneta': 'Veneta',
  'veneta': 'Veneta',
  'novo seculo': 'Novo Século',
  'salvat': 'Salvat',
  'eaglemoss': 'Eaglemoss',
  'devir': 'Devir',
  'devir livraria': 'Devir',
  'record': 'Record'
};

const RUIDOS_TITULO = [
  'edicao definitiva', 'edicao de luxo', 'deluxe edition', 'edicao especial',
  'capa dura', 'omnibus', 'graphic novel', 'volume', 'vol.', 'vol', 'edicao', 'ed.', 'ed'
];

export function normalizarTituloJev(s?: string | null): string {
  if (!s) return '';
  let limpo = s
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^\w\s]/gi, ' ');

  for (const ruido of RUIDOS_TITULO) {
    limpo = limpo.replace(new RegExp(`\\b${ruido}\\b`, 'gi'), ' ');
  }

  return limpo.replace(/\s+/g, ' ').trim();
}

export function normalizarEditoraJev(editora?: string | null): string {
  if (!editora) return '';
  const limpo = editora.trim().toLowerCase();

  if (EDITORAS_CANONICAS[limpo]) {
    return EDITORAS_CANONICAS[limpo];
  }

  for (const key of Object.keys(EDITORAS_CANONICAS)) {
    if (limpo.includes(key)) {
      return EDITORAS_CANONICAS[key];
    }
  }

  return editora.trim();
}

export function calcularLevenshteinRatio(s: string, t: string): number {
  if (!s && !t) return 1.0;
  if (!s || !t) return 0.0;
  if (s === t) return 1.0;

  const n = s.length;
  const m = t.length;
  const d: number[][] = [];

  for (let i = 0; i <= n; i++) {
    d[i] = [i];
  }
  for (let j = 0; j <= m; j++) {
    d[0][j] = j;
  }

  for (let i = 1; i <= n; i++) {
    for (let j = 1; j <= m; j++) {
      const cost = t[j - 1] === s[i - 1] ? 0 : 1;
      d[i][j] = Math.min(
        d[i - 1][j] + 1,
        d[i][j - 1] + 1,
        d[i - 1][j - 1] + cost
      );
    }
  }

  const dist = d[n][m];
  const maxLen = Math.max(n, m);
  return 1.0 - (dist / maxLen);
}

export function calcularJaroWinkler(s1: string, s2: string): number {
  if (s1 === s2) return 1.0;
  if (!s1 || !s2) return 0.0;

  const len1 = s1.length;
  const len2 = s2.length;
  const matchDistance = Math.floor(Math.max(len1, len2) / 2) - 1;

  const s1Matches = new Array(len1).fill(false);
  const s2Matches = new Array(len2).fill(false);

  let matches = 0;
  for (let i = 0; i < len1; i++) {
    const start = Math.max(0, i - matchDistance);
    const end = Math.min(i + matchDistance + 1, len2);

    for (let j = start; j < end; j++) {
      if (s2Matches[j]) continue;
      if (s1[i] !== s2[j]) continue;
      s1Matches[i] = true;
      s2Matches[j] = true;
      matches++;
      break;
    }
  }

  if (matches === 0) return 0.0;

  let transpositions = 0;
  let k = 0;
  for (let i = 0; i < len1; i++) {
    if (!s1Matches[i]) continue;
    while (!s2Matches[k]) k++;
    if (s1[i] !== s2[k]) transpositions++;
    k++;
  }

  const jaro = ((matches / len1) + (matches / len2) + ((matches - transpositions / 2) / matches)) / 3.0;

  // Winkler prefix
  let prefix = 0;
  for (let i = 0; i < Math.min(4, Math.min(len1, len2)); i++) {
    if (s1[i] === s2[i]) prefix++;
    else break;
  }

  return jaro + (prefix * 0.1 * (1.0 - jaro));
}

export function calcularTokenSetRatio(s1: string, s2: string): number {
  const t1 = new Set(s1.split(' ').filter(w => w.length > 0));
  const t2 = new Set(s2.split(' ').filter(w => w.length > 0));

  if (t1.size === 0 && t2.size === 0) return 1.0;
  if (t1.size === 0 || t2.size === 0) return 0.0;

  let interCount = 0;
  for (const w of t1) {
    if (t2.has(w)) interCount++;
  }

  const unionSize = t1.size + t2.size - interCount;
  return unionSize > 0 ? (interCount / unionSize) : 0;
}

export interface JevMatchResult {
  scoreGeral: number;
  scoreTitulo: number;
  scoreEditora: number;
  scoreEdicao: number;
  classificacao: 'Duplicata Confirmada' | 'Mesma Série (Outra Edição)' | 'Possível Variante / Reimpressão' | 'Semelhante' | 'Distinto';
  motivo: string;
}

export function calcularSimilaridadeProbabilisticaJev(
  tituloA: string,
  editoraA?: string,
  edicaoA: number = 1,
  tituloB: string = '',
  editoraB?: string,
  edicaoB: number = 1
): JevMatchResult {
  const normA = normalizarTituloJev(tituloA);
  const normB = normalizarTituloJev(tituloB);

  if (!normA || !normB) {
    return {
      scoreGeral: 0,
      scoreTitulo: 0,
      scoreEditora: 0,
      scoreEdicao: 0,
      classificacao: 'Distinto',
      motivo: 'Título em branco'
    };
  }

  // 1. Título (Token Set Ratio 50% + Jaro-Winkler 30% + Levenshtein 20%)
  const tokenSet = calcularTokenSetRatio(normA, normB);
  const jaro = calcularJaroWinkler(normA, normB);
  const lev = calcularLevenshteinRatio(normA, normB);

  let scoreTitulo = Math.round(((tokenSet * 0.50) + (jaro * 0.30) + (lev * 0.20)) * 100);
  scoreTitulo = Math.min(100, Math.max(0, scoreTitulo));

  // 2. Edição
  const numA = edicaoA <= 0 ? 1 : edicaoA;
  const numB = edicaoB <= 0 ? 1 : edicaoB;
  const scoreEdicao = (numA === numB) ? 100 : 0;

  // 3. Editora
  const canEdA = normalizarEditoraJev(editoraA);
  const canEdB = normalizarEditoraJev(editoraB);
  let scoreEditora = 60;

  if (canEdA && canEdB) {
    if (canEdA.toLowerCase() === canEdB.toLowerCase()) {
      scoreEditora = 100;
    } else if (canEdA.toLowerCase().includes(canEdB.toLowerCase()) || canEdB.toLowerCase().includes(canEdA.toLowerCase())) {
      scoreEditora = 85;
    } else {
      scoreEditora = 15;
    }
  }

  // 4. Score Composto JEV (55% Título + 25% Edição + 20% Editora)
  let scoreGeral = Math.round((scoreTitulo * 0.55) + (scoreEdicao * 0.25) + (scoreEditora * 0.20));

  if (scoreTitulo >= 98 && scoreEdicao === 100) {
    scoreGeral = Math.max(scoreGeral, 97);
  }

  let classificacao: JevMatchResult['classificacao'];
  let motivo: string;

  if (scoreGeral >= 88) {
    classificacao = 'Duplicata Confirmada';
    motivo = `Alta probabilidade de duplicata (${scoreGeral}%). Título e edição idênticos ou variantes da mesma editora.`;
  } else if (scoreTitulo >= 80 && scoreEdicao === 0) {
    classificacao = 'Mesma Série (Outra Edição)';
    motivo = `Pertence à mesma série/coleção (${scoreTitulo}% de similaridade), porém edição #${numB} vs #${numA}.`;
  } else if (scoreGeral >= 68 || (scoreTitulo >= 75 && scoreEditora >= 60)) {
    classificacao = 'Possível Variante / Reimpressão';
    motivo = `Forte correlação (${scoreGeral}%). Reimpressão, encadernado correlato ou edição especial.`;
  } else if (scoreGeral >= 48) {
    classificacao = 'Semelhante';
    motivo = `Palavras-chave em comum no título ou editora (${scoreGeral}%).`;
  } else {
    classificacao = 'Distinto';
    motivo = 'Baixa probabilidade de correlação.';
  }

  return {
    scoreGeral,
    scoreTitulo,
    scoreEditora,
    scoreEdicao,
    classificacao,
    motivo
  };
}

/**
 * Executa a busca e ranqueamento probabilístico JEV sobre um acervo de HQs local em 0ms.
 */
export function buscarColecaoLocalJev(
  queryTitulo: string,
  queryEditora: string | undefined = '',
  queryEdicao: number = 1,
  hqs: HQ[] = [],
  maxResultados: number = 25
): JevColecaoMatch[] {
  if (!queryTitulo || !hqs || hqs.length === 0) return [];

  const normQuery = normalizarTituloJev(queryTitulo);
  const tokensQuery = new Set(normQuery.split(' ').filter(w => w.length > 2));
  const matches: JevColecaoMatch[] = [];

  for (const hq of hqs) {
    const normHq = normalizarTituloJev(hq.titulo);
    if (!normHq) continue;

    // Pré-filtro rápido para descartar itens sem qualquer relação
    const containsSub = normQuery.includes(normHq) || normHq.includes(normQuery);
    let hasCommonWord = false;

    if (!containsSub && tokensQuery.size > 0) {
      const tokensHq = normHq.split(' ');
      for (const t of tokensHq) {
        if (tokensQuery.has(t)) {
          hasCommonWord = true;
          break;
        }
      }
    }

    if (!containsSub && !hasCommonWord && Math.abs(normQuery.length - normHq.length) > 15) {
      continue;
    }

    const res = calcularSimilaridadeProbabilisticaJev(
      queryTitulo,
      queryEditora,
      queryEdicao,
      hq.titulo,
      hq.editora,
      hq.numeroEdicao || 1
    );

    if (res.scoreGeral >= 45) {
      matches.push({
        id: hq.id,
        titulo: hq.titulo,
        editora: hq.editora || '—',
        numeroEdicao: hq.numeroEdicao || 1,
        lido: hq.lido || 0,
        imagem: (hq as any).thumbCapa || (hq as any).capa,
        scoreGeral: res.scoreGeral,
        scoreTitulo: res.scoreTitulo,
        scoreEditora: res.scoreEditora,
        scoreEdicao: res.scoreEdicao,
        classificacao: res.classificacao,
        motivo: res.motivo
      });
    }
  }

  matches.sort((a, b) => b.scoreGeral - a.scoreGeral || b.scoreTitulo - a.scoreTitulo);
  return matches.slice(0, maxResultados);
}
