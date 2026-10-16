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

