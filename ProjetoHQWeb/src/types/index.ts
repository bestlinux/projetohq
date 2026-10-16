export interface HQ {
  id: string;
  editora: string;
  titulo: string;
  numeroEdicao: number;
  categoria?: number;
  genero?: number;
  status?: number;
  formato?: number;
  linkDetalhes?: string;
  thumbCapa?: string;
  licenciador?: string;
  numeroPaginas?: number;
  preco?: string;
  dataPublicacao?: string;
  capa?: string;
  personagens?: string;
  desenhosRoteirosArteFinalCores?: string;
  lido: number; // 0 = Não lido, 1 = Lido
}

export interface Desejo {
  id: string;
  editora: string;
  titulo: string;
  numeroEdicao: number;
  categoria?: number;
  genero?: number;
  status?: number;
  formato?: number;
  linkDetalhes?: string;
  thumbCapa?: string;
  licenciador?: string;
  numeroPaginas?: number;
  preco?: string;
  dataPublicacao?: string;
  capa?: string;
  personagens?: string;
  desenhosRoteirosArteFinalCores?: string;
  lido: number;
}

export interface Editora {
  id: string;
  nome: string;
}

export interface Leitura {
  id: string;
  titulo: string;
  capa?: string;
  lido: number; // 0 = Não lido / Na fila, 1 = Lido
  dataPublicacao?: string;
  dataLeitura?: string | null;
}

export interface Frase {
  id: string;
  nomeHQ: string;
  autor: string;
  arquivo: string;
}

export interface UserToken {
  token: string;
  expiration: string;
}

export interface ImportacaoLegado {
  id: string;
  titulo: string;
  editora: string;
  edicao: number;
  created?: string;
  lastModified?: string;
}

export interface ImportacaoLegadoResult {
  success: boolean;
  totalLidos: number;
  jaCadastrados: number;
  novosImportados: number;
  duplicadosIgnorados: number;
  mensagem: string;
}

// ----------------------------------------
// CURADOR DE HQS - AGENTES DE IA (GEMINI)
// ----------------------------------------

export interface ItemOrdemLeitura {
  ordem: number;
  titulo: string;
  editoraRecomendada: string;
  numeroEdicao: number;
  importancia: string;
  sinopseCurta: string;
  naEstante: boolean;
  hqId?: string | null;
  lido: boolean;
}

export interface OrdemLeituraResponse {
  saga: string;
  universo: string;
  descricaoGeral: string;
  itens: ItemOrdemLeitura[];
  totalItensNaEstante: number;
  totalItensAusentes: number;
}

export interface MetricaPercentual {
  nome: string;
  quantidade: number;
  porcentagem: number;
}

export interface GapSerie {
  serie: string;
  editora: string;
  edicoesPossuidas: number[];
  edicoesFaltantes: number[];
  urgencia: string;
  motivoGap: string;
}

export interface RecomendacaoAquisicao {
  titulo: string;
  editora: string;
  numeroEdicao: number;
  motivo: string;
  matchScore: string;
}

export interface DnaColecaoResponse {
  arquetipo: string;
  subtituloArquetipo: string;
  descricaoArquetipo: string;
  totalHqs: number;
  totalLidas: number;
  porcentagemLidas: number;
  distribuicaoEditoras: MetricaPercentual[];
  distribuicaoGeneros: MetricaPercentual[];
  gapsDetectados: GapSerie[];
  recomendacoesProximasAquisicoes: RecomendacaoAquisicao[];
  pontosFortesColecao: string[];
}

export interface RecapNarrativoResponse {
  titulo: string;
  numeroEdicao: number;
  arcoNarrativo: string;
  previouslyOnText: string;
  personagensChave: string[];
  eventosPrecedentes: string[];
  climaOuTom: string;
}

export interface PerguntaQuiz {
  id: number;
  pergunta: string;
  opcoes: string[];
  respostaCorretaIndex: number;
  explicacao: string;
  hqReferencia: string;
}

export interface QuizAcervoResponse {
  perguntas: PerguntaQuiz[];
}

export interface ChatMessage {
  remetente: 'user' | 'model';
  texto: string;
  origemMotor?: string;
  tempoProcessamentoMs?: number;
}

export interface ChatbotResponse {
  resposta: string;
  hqsMencionadas: string[];
  sugestoesPerguntas: string[];
  origemMotor?: string; // "JEV_SYSTEM1_LOCAL" | "GEMINI_SYSTEM2_CLOUD"
  tempoProcessamentoMs?: number;
}

export interface ComandoCrudPreview {
  comandoOriginal: string;
  acao: string;
  interpretacaoHumana: string;
  entidadeAlvo: string;
  hqIdAlvo?: string | null;
  tituloAlvo?: string;
  editoraAlvo?: string;
  edicaoAlvo?: number;
  parametrosModificadosJson?: string;
  requerConfirmacao: boolean;
  origemMotor?: string; // "JEV_SYSTEM1_LOCAL" | "GEMINI_SYSTEM2_CLOUD"
  tempoProcessamentoMs?: number;
}

export interface JevColecaoMatch {
  id: string;
  titulo: string;
  editora: string;
  numeroEdicao: number;
  lido: number;
  imagem?: string;
  scoreGeral: number;
  scoreTitulo: number;
  scoreEditora: number;
  scoreEdicao: number;
  classificacao: string;
  motivo: string;
}

export interface ComandoCrudResult {
  sucesso: boolean;
  mensagem: string;
  idRegistroAfetado?: string | null;
}

export interface ItemLotePreview {
  idTemp: number;
  titulo: string;
  editora: string;
  numeroEdicao: number;
  anoLancamento?: string;
  categoria: number;
  genero: number;
  precoSugerido: string;
  jaExisteNaColecao: boolean;
  selecionado: boolean;
}

export interface ImportacaoLotePreview {
  totalDetectados: number;
  itens: ItemLotePreview[];
}

export interface ImportacaoLoteSalvarResult {
  totalSalvos: number;
  mensagem: string;
}

export interface MetricaPorFuncionalidade {
  funcionalidade: string;
  chamadas: number;
  tokens: number;
  custoUSD: number;
  latenciaMediaMs: number;
  porcentagemUso: number;
}

export interface EvolucaoDiariaTelemetria {
  data: string;
  chamadasGemini: number;
  chamadasJev: number;
  tokens: number;
  custoUSD: number;
}

export interface ResumoTelemetriaIa {
  totalChamadas: number;
  chamadasComSucesso: number;
  chamadasComErro: number;
  taxaSucessoPercentual: number;
  totalChamadasGemini: number;
  totalChamadasJev: number;
  totalPromptTokens: number;
  totalCompletionTokens: number;
  totalTokens: number;
  totalFerramentas: number;
  latenciaMediaMs: number;
  latenciaP95Ms: number;
  latenciaMediaGeminiMs: number;
  latenciaMediaJevMs: number;
  custoTotalUSD: number;
  custoTotalBRL: number;
  custoPorRequisicaoSucessoUSD: number;
  custoPorRequisicaoSucessoBRL: number;
  totalEconomizadoJevUSD: number;
  totalEconomizadoJevBRL: number;
  tokensEconomizadosJev: number;
  distribuicaoPorFuncionalidade: MetricaPorFuncionalidade[];
  evolucaoDiaria: EvolucaoDiariaTelemetria[];
}

export interface LogUsoIAItem {
  id: string;
  dataHora: string;
  funcionalidade: string;
  operacao: string;
  modelo: string;
  origemMotor: string;
  promptTokens: number;
  completionTokens: number;
  totalTokens: number;
  qtdFerramentas: number;
  latenciaMs: number;
  sucesso: boolean;
  codigoStatus?: number;
  mensagemErro?: string;
  custoEstimadoUSD: number;
  custoEconomizadoUSD: number;
}



