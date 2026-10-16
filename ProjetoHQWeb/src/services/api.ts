import { HQ, Desejo, Editora, Leitura, Frase, ImportacaoLegado, ImportacaoLegadoResult } from '../types';
import { INITIAL_HQS, INITIAL_DESEJOS, INITIAL_EDITORAS } from './mockData';

const STORAGE_KEYS = {
  HQS: 'projetohq_hqs_v1',
  DESEJOS: 'projetohq_desejos_v1',
  EDITORAS: 'projetohq_editoras_v1',
  LEITURAS: 'projetohq_leituras_v1',
  FRASES: 'projetohq_frases_v1',
  API_URL: 'projetohq_api_url',
  TOKEN: 'projetohq_token',
};

export const getStoredApiUrl = (): string => {
  return localStorage.getItem(STORAGE_KEYS.API_URL) || '/api';
};

export const setStoredApiUrl = (url: string) => {
  localStorage.setItem(STORAGE_KEYS.API_URL, url);
};

export const getStoredToken = (): string | null => {
  return localStorage.getItem(STORAGE_KEYS.TOKEN);
};

export const setStoredToken = (token: string | null) => {
  if (token) {
    localStorage.setItem(STORAGE_KEYS.TOKEN, token);
  } else {
    localStorage.removeItem(STORAGE_KEYS.TOKEN);
  }
};

/**
 * Converte o campo 'Capa' (que armazena o id/nome do arquivo em projetohqrepositorio\Capas)
 * para uma URL utilizável pelo FrontEnd através da API ou proxy.
 */
export const getCapaUrl = (capaId?: string | null): string | undefined => {
  if (!capaId || typeof capaId !== 'string') return undefined;
  const clean = capaId.trim();
  if (!clean || clean === '0' || clean.toLowerCase() === 'null') return undefined;

  // Se já for uma URL externa (http:// ou https://)
  if (clean.startsWith('http://') || clean.startsWith('https://')) {
    return clean;
  }

  // Se for o ID/nome do arquivo local
  let filename = clean;
  if (!filename.toLowerCase().endsWith('.jpg') && !filename.toLowerCase().endsWith('.jpeg') && !filename.toLowerCase().endsWith('.png')) {
    filename += '.jpg';
  }

  return `/capas/${encodeURIComponent(filename)}`;
};

/**
 * Converte o campo 'Capa' da tabela Desejos (armazenado em projetohqrepositorio\Desejos)
 * para uma URL utilizável pelo FrontEnd através da API ou proxy.
 */
export const getCapaDesejoUrl = (capaId?: string | null): string | undefined => {
  if (!capaId || typeof capaId !== 'string') return undefined;
  const clean = capaId.trim();
  if (!clean || clean === '0' || clean.toLowerCase() === 'null') return undefined;

  // Se já for uma URL externa (http:// ou https://)
  if (clean.startsWith('http://') || clean.startsWith('https://')) {
    return clean;
  }

  // Se for o ID/nome do arquivo local
  let filename = clean;
  if (!filename.toLowerCase().endsWith('.jpg') && !filename.toLowerCase().endsWith('.jpeg') && !filename.toLowerCase().endsWith('.png')) {
    filename += '.jpg';
  }

  return `/desejos-capas/${encodeURIComponent(filename)}`;
};

/**
 * Converte o campo 'Arquivo' da tabela Frases (armazenado em projetohqrepositorio\Frases)
 * para uma URL utilizável pelo FrontEnd através da API ou proxy.
 */
export const getFraseImagemUrl = (arquivo?: string | null): string | undefined => {
  if (!arquivo || typeof arquivo !== 'string') return undefined;
  const clean = arquivo.trim();
  if (!clean || clean === '0' || clean.toLowerCase() === 'null') return undefined;

  if (clean.startsWith('http://') || clean.startsWith('https://')) {
    return clean;
  }

  let filename = clean;
  return `/frases-imagens/${encodeURIComponent(filename)}`;
};

// Helper for local storage persistence
function getLocal<T>(key: string, initial: T): T {
  const data = localStorage.getItem(key);
  if (!data) {
    try {
      localStorage.setItem(key, JSON.stringify(initial));
    } catch {
      // quota or private browsing
    }
    return initial;
  }
  try {
    return JSON.parse(data) as T;
  } catch {
    return initial;
  }
}

function setLocal<T>(key: string, val: T): void {
  // Desacoplado da thread principal para não bloquear a renderização com grandes conjuntos de dados (>3.000 itens)
  setTimeout(() => {
    try {
      localStorage.setItem(key, JSON.stringify(val));
    } catch {
      // Limite de cota de 5MB atingido no navegador - mantido no memoryCache
    }
  }, 10);
}

// In-memory cache to support large datasets (>4MB) without crashing localStorage quotas
let memoryCacheHQs: HQ[] | null = null;
let memoryCacheEditoras: Editora[] | null = null;
let memoryCacheDesejos: Desejo[] | null = null;
let memoryCacheLeituras: Leitura[] | null = null;
let memoryCacheFrases: Frase[] | null = null;

let activeLoginPromise: Promise<string | null> | null = null;

export const ensureAuthenticated = async (): Promise<string | null> => {
  let token = getStoredToken();
  if (token) return token;

  if (activeLoginPromise) {
    return activeLoginPromise;
  }

  // Deduplicação: se múltiplas chamadas pedirem autenticação ao mesmo tempo, compartilham a mesma Promise
  activeLoginPromise = (async () => {
    try {
      const loginResult = await apiService.login('bestlinux@gmail.com', '200314Amor@');
      if (loginResult.success && loginResult.token) {
        return loginResult.token;
      }
    } catch (err) {
      console.warn('[Auth] Tentativa de login automático falhou:', err);
    } finally {
      activeLoginPromise = null;
    }
    return null;
  })();

  return activeLoginPromise;
};

export const apiService = {
  // Test connection to backend
  async pingBackend(): Promise<{ success: boolean; message: string; authenticated: boolean; totalHQs?: number }> {
    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      const res = await fetch(`${baseUrl}/editora`, {
        headers: { 
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(6000),
      });

      if (res.ok) {
        return { 
          success: true, 
          authenticated: true,
          message: 'Conectado à API .NET 10 e autenticado no SQL Server com sucesso!' 
        };
      }
      
      if (res.status === 401) {
        return { 
          success: true, 
          authenticated: false, 
          message: 'API .NET 10 respondendo, mas requer autenticação.' 
        };
      }

      return { 
        success: false, 
        authenticated: false, 
        message: `Status HTTP ${res.status}` 
      };
    } catch (err: any) {
      return { 
        success: false, 
        authenticated: false, 
        message: 'Não foi possível alcançar a API (.NET 10 não está em execução ou URL inacessível).' 
      };
    }
  },

  // Authentication
  async login(email: string, password: string): Promise<{ success: boolean; message: string; token?: string }> {
    try {
      const baseUrl = getStoredApiUrl();
      const res = await fetch(`${baseUrl}/login/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({ email, password }),
        signal: AbortSignal.timeout(8000),
      });

      if (res.ok) {
        const data = await res.json();
        if (data.token) {
          setStoredToken(data.token);
          return { success: true, message: 'Autenticado com sucesso!', token: data.token };
        }
      }
      return { success: false, message: 'Credenciais inválidas ou erro no login.' };
    } catch (err: any) {
      return { success: false, message: 'Erro ao conectar ao endpoint de login: ' + err.message };
    }
  },

  logout() {
    setStoredToken(null);
    memoryCacheHQs = null;
    memoryCacheEditoras = null;
    memoryCacheDesejos = null;
    memoryCacheLeituras = null;
    memoryCacheFrases = null;
  },

  isAuthenticated(): boolean {
    return !!getStoredToken();
  },

  getCachedHQs(): HQ[] | null {
    if (memoryCacheHQs && memoryCacheHQs.length > 0) {
      return memoryCacheHQs;
    }
    const local = getLocal<HQ[]>(STORAGE_KEYS.HQS, []);
    if (local && local.length > 0) {
      memoryCacheHQs = local;
      return local;
    }
    return null;
  },

  computeEstatisticasLocal(list: HQ[]) {
    const totalLidas = list.filter(h => h.lido === 1).length;
    const editorasSet = new Set(list.map(h => (h.editora || '').trim()).filter(Boolean));
    const totalPaginas = list.reduce((acc, h) => acc + (Number(h.paginas || h.numeroPaginas) || 0), 0);
    const valorTotalEstimado = list.reduce((acc, h) => {
      if (!h.preco) return acc;
      if (typeof h.preco === 'number') return acc + h.preco;
      const clean = String(h.preco).replace(/R\$/gi, '').replace(/\$/g, '').trim().replace(/\./g, '').replace(',', '.');
      const val = parseFloat(clean);
      return acc + (isNaN(val) ? 0 : val);
    }, 0);

    return {
      totalHQs: list.length,
      totalLidas,
      totalNaoLidas: list.length - totalLidas,
      totalEditoras: editorasSet.size,
      totalPaginas,
      valorTotalEstimado: Math.round(valorTotalEstimado * 100) / 100
    };
  },

  // HQs
  async getHQs(forceRefresh = false): Promise<HQ[]> {
    if (!forceRefresh && memoryCacheHQs && memoryCacheHQs.length > 0) {
      return memoryCacheHQs;
    }

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/hq`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(90000),
      });

      // Se token expirou (401), refaz login e tenta mais uma vez
      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/hq`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(90000),
          });
        }
      }

      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data) && data.length > 0) {
          // Normalize API data to our frontend HQ model
          const mapped: HQ[] = data.map((item: any) => {
            const rawCapa = item.capa || item.Capa;
            const resolvedCover = getCapaUrl(rawCapa) || item.thumbCapa || undefined;

            return {
              id: item.id || crypto.randomUUID(),
              titulo: item.titulo || 'Sem Título',
              editora: item.editora || 'Independente',
              numeroEdicao: item.numeroEdicao ?? 1,
              dataLancamento: item.anoLancamento ? `${item.anoLancamento}-01-01` : item.dataLancamento,
              thumbCapa: resolvedCover,
              capa: rawCapa,
              lido: item.lido === true || item.lido === 1 ? 1 : 0,
              preco: item.preco || 0,
              paginas: item.paginas || item.numeroPaginas || 0,
              personagens: item.personagens || '',
              desenhosRoteirosArteFinalCores: item.desenhosRoteirosArteFinalCores || item.desenho || item.roteiro || '',
              descricao: item.descricao || ''
            };
          });

          memoryCacheHQs = mapped;
          setLocal(STORAGE_KEYS.HQS, mapped);
          return mapped;
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao buscar HQs da API, usando cache local:', err);
    }

    if (memoryCacheHQs && memoryCacheHQs.length > 0) {
      return memoryCacheHQs;
    }
    const local = getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    memoryCacheHQs = local;
    return local;
  },

  async toggleLido(hqId: string): Promise<HQ> {
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    const item = list.find(h => h.id === hqId);
    if (item) {
      item.lido = item.lido === 1 ? 0 : 1;
      setLocal(STORAGE_KEYS.HQS, [...list]);
      
      // Try notify backend
      try {
        const baseUrl = getStoredApiUrl();
        const token = await ensureAuthenticated();
        if (token) {
          await fetch(`${baseUrl}/hq/${hqId}/lido?lido=${item.lido}`, {
            method: 'PATCH',
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
        }
      } catch {
        // Ignored in offline/local
      }
      return item;
    }
    throw new Error('HQ não encontrada');
  },

  async uploadCapa(hqId: string, file: File): Promise<{ id: string; capa: string; url: string }> {
    const baseUrl = getStoredApiUrl();
    const token = await ensureAuthenticated();
    const formData = new FormData();
    formData.append('file', file);

    const res = await fetch(`${baseUrl}/hq/${hqId}/capa`, {
      method: 'POST',
      headers: {
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
      },
      body: formData
    });

    if (!res.ok) {
      throw new Error(`Erro ao enviar capa: ${res.statusText}`);
    }

    const data = await res.json();
    
    // Atualiza localmente
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    const found = list.find(h => h.id === hqId);
    if (found) {
      found.capa = data.capa;
      found.thumbCapa = data.url;
      setLocal(STORAGE_KEYS.HQS, [...list]);
    }
    return data;
  },

  async getEstatisticas(): Promise<{
    totalHQs: number;
    totalLidas: number;
    totalNaoLidas: number;
    totalEditoras: number;
    totalPaginas: number;
    valorTotalEstimado: number;
  }> {
    // Se já temos a lista carregada em memória, calcula instantaneamente sem bater no backend
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, []);
    if (list && list.length > 0) {
      return this.computeEstatisticasLocal(list);
    }

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      const res = await fetch(`${baseUrl}/hq/estatisticas`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
      });
      if (res.ok) {
        return await res.json();
      }
    } catch (err) {
      console.warn('Erro ao carregar estatísticas do backend:', err);
    }
    return this.computeEstatisticasLocal(INITIAL_HQS);
  },

  async addHQ(hq: Omit<HQ, 'id'>): Promise<HQ> {
    const newHQ: HQ = {
      ...hq,
      id: crypto.randomUUID(),
    };
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    const updated = [newHQ, ...list];
    memoryCacheHQs = updated;
    setLocal(STORAGE_KEYS.HQS, updated);

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      if (token) {
        await fetch(`${baseUrl}/hq`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify([newHQ])
        });
      }
    } catch {
      // Saved locally
    }
    return newHQ;
  },

  async updateHQ(hq: HQ): Promise<HQ> {
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    const updated = list.map(item => item.id === hq.id ? hq : item);
    memoryCacheHQs = updated;
    setLocal(STORAGE_KEYS.HQS, updated);

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      if (token) {
        await fetch(`${baseUrl}/hq/${hq.id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(hq)
        });
      }
    } catch (err) {
      console.warn('Erro ao atualizar HQ no backend:', err);
    }
    return hq;
  },

  async buscaWeb(params: {
    titulo: string;
    editora?: string;
    categoria?: number;
    genero?: number;
    status?: number;
    formato?: number;
    numeroEdicao?: number;
    anoLancamento?: string;
  }): Promise<HQ[]> {
    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      const tit = encodeURIComponent(params.titulo.trim());
      const ed = encodeURIComponent((params.editora || 'Todas').trim());
      const cat = params.categoria ?? 0;
      const gen = params.genero ?? 0;
      const st = params.status ?? 0;
      const form = params.formato ?? 0;
      const num = params.numeroEdicao ?? 0;
      const ano = params.anoLancamento ? encodeURIComponent(params.anoLancamento.trim()) : '';

      const url = `${baseUrl}/hq/BuscaWeb/${tit}/${ed}/${cat}/${gen}/${st}/${form}/${num}/${ano}`;
      const res = await fetch(url, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
      });
      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data)) {
          return data.map((item: any) => ({
            id: item.id || crypto.randomUUID(),
            titulo: item.titulo || params.titulo,
            editora: item.editora || params.editora || 'Independente',
            numeroEdicao: item.numeroEdicao || 1,
            dataPublicacao: item.dataPublicacao || item.anoLancamento,
            thumbCapa: getCapaUrl(item.capa) || item.thumbCapa,
            capa: item.capa,
            lido: 0,
            preco: item.preco || '',
            numeroPaginas: item.numeroPaginas || 0,
            personagens: item.personagens || '',
            desenhosRoteirosArteFinalCores: item.desenhosRoteirosArteFinalCores || '',
            linkDetalhes: item.linkDetalhes
          }));
        }
      }
    } catch (err) {
      console.warn('Erro ao buscar HQs na web:', err);
    }
    return [];
  },

  async removeHQ(hqId: string): Promise<void> {
    const list = memoryCacheHQs || getLocal<HQ[]>(STORAGE_KEYS.HQS, INITIAL_HQS);
    const filtered = list.filter(h => h.id !== hqId);
    memoryCacheHQs = filtered;
    setLocal(STORAGE_KEYS.HQS, filtered);

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      if (token) {
        await fetch(`${baseUrl}/hq/${hqId}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });
      }
    } catch {
      // Removed locally
    }
  },

  // Desejos
  async getDesejos(): Promise<Desejo[]> {
    if (memoryCacheDesejos && memoryCacheDesejos.length > 0) {
      return memoryCacheDesejos;
    }

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      const res = await fetch(`${baseUrl}/desejo`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(6000),
      });

      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data) && data.length > 0) {
          const mapped: Desejo[] = data.map((item: any) => {
            const rawCapa = item.capa || item.Capa;
            const resolvedCover = getCapaDesejoUrl(rawCapa) || item.thumbCapa || undefined;

            return {
              id: item.id || crypto.randomUUID(),
              titulo: item.titulo || 'Sem Título',
              editora: item.editora || 'Independente',
              numeroEdicao: item.numeroEdicao ?? 1,
              thumbCapa: resolvedCover,
              capa: rawCapa,
              lido: 0,
              preco: item.preco || 0,
              personagens: item.personagens || '',
              desenhosRoteirosArteFinalCores: item.desenhosRoteirosArteFinalCores || ''
            };
          });
          memoryCacheDesejos = mapped;
          setLocal(STORAGE_KEYS.DESEJOS, mapped);
          return mapped;
        }
      }
    } catch {
      // Fallback
    }
    return getLocal<Desejo[]>(STORAGE_KEYS.DESEJOS, INITIAL_DESEJOS);
  },

  async addDesejo(desejo: Omit<Desejo, 'id'>): Promise<Desejo> {
    const newDesejo: Desejo = {
      ...desejo,
      id: crypto.randomUUID(),
    };
    const list = memoryCacheDesejos || getLocal<Desejo[]>(STORAGE_KEYS.DESEJOS, INITIAL_DESEJOS);
    const updated = [newDesejo, ...list];
    memoryCacheDesejos = updated;
    setLocal(STORAGE_KEYS.DESEJOS, updated);
    return newDesejo;
  },

  async removeDesejo(desejoId: string): Promise<void> {
    const list = memoryCacheDesejos || getLocal<Desejo[]>(STORAGE_KEYS.DESEJOS, INITIAL_DESEJOS);
    const filtered = list.filter(d => d.id !== desejoId);
    memoryCacheDesejos = filtered;
    setLocal(STORAGE_KEYS.DESEJOS, filtered);

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        let res = await fetch(`${baseUrl}/desejo/${desejoId}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/desejo/${desejoId}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${token}`
              }
            });
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao excluir desejo da base remota:', err);
    }
  },

  // Editoras
  async getEditoras(forceRefresh = false): Promise<Editora[]> {
    if (!forceRefresh && memoryCacheEditoras && memoryCacheEditoras.length > 0) {
      return memoryCacheEditoras;
    }

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      let res = await fetch(`${baseUrl}/editora`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(6000),
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/editora`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(6000),
          });
        }
      }

      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data) && data.length > 0) {
          const mapped: Editora[] = data.map((item: any) => ({
            id: item.id || crypto.randomUUID(),
            nome: item.nome || item.name || ''
          }));
          mapped.sort((a, b) => a.nome.localeCompare(b.nome));
          memoryCacheEditoras = mapped;
          setLocal(STORAGE_KEYS.EDITORAS, mapped);
          return mapped;
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao buscar Editoras da API:', err);
    }
    return getLocal<Editora[]>(STORAGE_KEYS.EDITORAS, INITIAL_EDITORAS);
  },

  async addEditora(nome: string): Promise<Editora> {
    const tempId = crypto.randomUUID();
    const newEditora: Editora = { id: tempId, nome: nome.trim() };

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      if (token) {
        const res = await fetch(`${baseUrl}/editora`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({ nome: nome.trim() })
        });

        if (res.ok) {
          const createdId = await res.json();
          if (createdId && typeof createdId === 'string') {
            newEditora.id = createdId;
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao cadastrar Editora na API:', err);
    }

    const list = memoryCacheEditoras || getLocal<Editora[]>(STORAGE_KEYS.EDITORAS, INITIAL_EDITORAS);
    const updated = [...list, newEditora].sort((a, b) => a.nome.localeCompare(b.nome));
    memoryCacheEditoras = updated;
    setLocal(STORAGE_KEYS.EDITORAS, updated);
    return newEditora;
  },

  async updateEditora(id: string, nome: string): Promise<Editora> {
    const updatedItem: Editora = { id, nome: nome.trim() };

    try {
      const baseUrl = getStoredApiUrl();
      const token = await ensureAuthenticated();
      if (token) {
        await fetch(`${baseUrl}/editora/${id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify({ id, nome: nome.trim() })
        });
      }
    } catch (err) {
      console.warn('[API] Erro ao atualizar Editora na API:', err);
    }

    const list = memoryCacheEditoras || getLocal<Editora[]>(STORAGE_KEYS.EDITORAS, INITIAL_EDITORAS);
    const updated = list.map(item => item.id === id ? updatedItem : item).sort((a, b) => a.nome.localeCompare(b.nome));
    memoryCacheEditoras = updated;
    setLocal(STORAGE_KEYS.EDITORAS, updated);
    return updatedItem;
  },

  async removeEditora(id: string): Promise<boolean> {
    const list = memoryCacheEditoras || getLocal<Editora[]>(STORAGE_KEYS.EDITORAS, INITIAL_EDITORAS);
    const filtered = list.filter(item => item.id !== id);
    memoryCacheEditoras = filtered;
    setLocal(STORAGE_KEYS.EDITORAS, filtered);

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        let res = await fetch(`${baseUrl}/editora/${id}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/editora/${id}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${token}`
              }
            });
          }
        }
        return true;
      }
    } catch (err) {
      console.warn('[API] Erro ao excluir Editora na API:', err);
    }
    return true;
  },

  // Leituras (api/Leitura)
  async getLeituras(titulo?: string, forceRefresh = false): Promise<Leitura[]> {
    if (!titulo && !forceRefresh && memoryCacheLeituras && memoryCacheLeituras.length > 0) {
      return memoryCacheLeituras;
    }

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      const queryParam = titulo && titulo.trim() ? `?Titulo=${encodeURIComponent(titulo.trim())}` : '';

      let res = await fetch(`${baseUrl}/leitura${queryParam}`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(10000),
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/leitura${queryParam}`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(10000),
          });
        }
      }

      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data)) {
          const mapped: Leitura[] = data.map((item: any) => ({
            id: item.id || crypto.randomUUID(),
            titulo: item.titulo || 'Sem Título',
            capa: item.capa || '',
            lido: item.lido === 1 || item.lido === true ? 1 : 0,
            dataPublicacao: item.dataPublicacao || '',
            dataLeitura: item.dataLeitura || null
          }));

          if (!titulo) {
            memoryCacheLeituras = mapped;
            setLocal(STORAGE_KEYS.LEITURAS, mapped);
          }
          return mapped;
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao buscar Leituras da API:', err);
    }

    if (memoryCacheLeituras && memoryCacheLeituras.length > 0) {
      return memoryCacheLeituras;
    }
    return getLocal<Leitura[]>(STORAGE_KEYS.LEITURAS, []);
  },

  async getLeituraById(id: string): Promise<Leitura | null> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      let res = await fetch(`${baseUrl}/leitura/${id}`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(8000),
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/leitura/${id}`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(8000),
          });
        }
      }

      if (res.ok) {
        const item = await res.json();
        return {
          id: item.id,
          titulo: item.titulo,
          capa: item.capa,
          lido: item.lido === 1 || item.lido === true ? 1 : 0,
          dataPublicacao: item.dataPublicacao,
          dataLeitura: item.dataLeitura
        };
      }
    } catch (err) {
      console.warn(`[API] Erro ao buscar Leitura ${id}:`, err);
    }

    const list = memoryCacheLeituras || getLocal<Leitura[]>(STORAGE_KEYS.LEITURAS, []);
    return list.find(item => item.id === id) || null;
  },

  async addLeitura(data: { titulo: string; capa?: string; dataPublicacao?: string }): Promise<Leitura> {
    const tempId = crypto.randomUUID();
    const newLeitura: Leitura = {
      id: tempId,
      titulo: data.titulo.trim(),
      capa: data.capa?.trim() || '',
      dataPublicacao: data.dataPublicacao?.trim() || '',
      lido: 0,
      dataLeitura: null
    };

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        const payload = {
          titulo: newLeitura.titulo,
          capa: newLeitura.capa,
          dataPublicacao: newLeitura.dataPublicacao,
          lido: 0
        };

        let res = await fetch(`${baseUrl}/leitura`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            res = await fetch(`${baseUrl}/leitura`, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
              },
              body: JSON.stringify(payload)
            });
          }
        }

        if (res.ok) {
          const createdId = await res.json();
          if (createdId && typeof createdId === 'string') {
            newLeitura.id = createdId;
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao criar Leitura na API:', err);
    }

    const list = memoryCacheLeituras || getLocal<Leitura[]>(STORAGE_KEYS.LEITURAS, []);
    const updated = [newLeitura, ...list.filter(item => item.id !== newLeitura.id)];
    memoryCacheLeituras = updated;
    setLocal(STORAGE_KEYS.LEITURAS, updated);
    return newLeitura;
  },

  async updateLeitura(id: string, lido: number, dataLeitura?: string | null): Promise<boolean> {
    const nowIso = new Date().toISOString();
    const resolvedDataLeitura = lido === 1 ? (dataLeitura || nowIso) : null;

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        const payload = {
          id: id,
          lido: lido,
          dataLeitura: resolvedDataLeitura || nowIso // API UpdateLeituraCommand expects non-null if model binder parses DateTime
        };

        let res = await fetch(`${baseUrl}/leitura/${id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/leitura/${id}`, {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
              },
              body: JSON.stringify(payload)
            });
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao atualizar Leitura na API:', err);
    }

    const list = memoryCacheLeituras || getLocal<Leitura[]>(STORAGE_KEYS.LEITURAS, []);
    const updated = list.map(item => {
      if (item.id === id) {
        return {
          ...item,
          lido,
          dataLeitura: resolvedDataLeitura
        };
      }
      return item;
    });
    memoryCacheLeituras = updated;
    setLocal(STORAGE_KEYS.LEITURAS, updated);
    return true;
  },

  async removeLeitura(id: string): Promise<boolean> {
    const list = memoryCacheLeituras || getLocal<Leitura[]>(STORAGE_KEYS.LEITURAS, []);
    const filtered = list.filter(item => item.id !== id);
    memoryCacheLeituras = filtered;
    setLocal(STORAGE_KEYS.LEITURAS, filtered);

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        let res = await fetch(`${baseUrl}/leitura/${id}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/leitura/${id}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${token}`
              }
            });
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao excluir Leitura na API:', err);
    }
    return true;
  },

  // Frases (api/Frases)
  async getFrases(filter?: { nomeHq?: string; autor?: string }, forceRefresh = false): Promise<Frase[]> {
    const hasFilter = filter && (filter.nomeHq || filter.autor);
    if (!hasFilter && !forceRefresh && memoryCacheFrases && memoryCacheFrases.length > 0) {
      return memoryCacheFrases;
    }

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      const params = new URLSearchParams();
      if (filter?.nomeHq?.trim()) params.append('NomeHQ', filter.nomeHq.trim());
      if (filter?.autor?.trim()) params.append('Autor', filter.autor.trim());
      const queryStr = params.toString() ? `?${params.toString()}` : '';

      let res = await fetch(`${baseUrl}/frases${queryStr}`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(10000),
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/frases${queryStr}`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(10000),
          });
        }
      }

      if (res.ok) {
        const data = await res.json();
        if (Array.isArray(data)) {
          const mapped: Frase[] = data.map((item: any) => ({
            id: item.id || crypto.randomUUID(),
            nomeHQ: item.nomeHQ || '',
            autor: item.autor || '',
            arquivo: item.arquivo || ''
          }));

          if (!hasFilter) {
            memoryCacheFrases = mapped;
            setLocal(STORAGE_KEYS.FRASES, mapped);
          }
          return mapped;
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao buscar Frases da API:', err);
    }

    if (memoryCacheFrases && memoryCacheFrases.length > 0) {
      return memoryCacheFrases;
    }
    return getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
  },

  async getFraseById(id: string): Promise<Frase | null> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      let res = await fetch(`${baseUrl}/frases/${id}`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(8000),
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/frases/${id}`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(8000),
          });
        }
      }

      if (res.ok) {
        const item = await res.json();
        return {
          id: item.id,
          nomeHQ: item.nomeHQ,
          autor: item.autor,
          arquivo: item.arquivo
        };
      }
    } catch (err) {
      console.warn(`[API] Erro ao buscar Frase ${id}:`, err);
    }

    const list = memoryCacheFrases || getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
    return list.find(item => item.id === id) || null;
  },

  async addFrase(data: { nomeHq: string; autor: string; arquivo: string }): Promise<Frase> {
    const tempId = crypto.randomUUID();
    const newFrase: Frase = {
      id: tempId,
      nomeHQ: data.nomeHq.trim(),
      autor: data.autor.trim(),
      arquivo: data.arquivo.trim()
    };

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        const payload = {
          nomeHQ: newFrase.nomeHQ,
          autor: newFrase.autor,
          arquivo: newFrase.arquivo
        };

        let res = await fetch(`${baseUrl}/frases`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            res = await fetch(`${baseUrl}/frases`, {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
              },
              body: JSON.stringify(payload)
            });
          }
        }

        if (res.ok) {
          const createdId = await res.json();
          if (createdId && typeof createdId === 'string') {
            newFrase.id = createdId;
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao criar Frase na API:', err);
    }

    const list = memoryCacheFrases || getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
    const updated = [newFrase, ...list.filter(item => item.id !== newFrase.id)];
    memoryCacheFrases = updated;
    setLocal(STORAGE_KEYS.FRASES, updated);
    return newFrase;
  },

  async uploadFraseImage(nomeHq: string, autor: string, file: File): Promise<Frase> {
    const tempId = crypto.randomUUID();
    const newFrase: Frase = {
      id: tempId,
      nomeHQ: nomeHq.trim(),
      autor: autor.trim(),
      arquivo: file.name
    };

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('NomeHQ', nomeHq.trim());
        formData.append('Autor', autor.trim());

        let res = await fetch(`${baseUrl}/frases/UploadImage`, {
          method: 'POST',
          headers: {
            ...(token ? { 'Authorization': `Bearer ${token}` } : {})
          },
          body: formData
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            res = await fetch(`${baseUrl}/frases/UploadImage`, {
              method: 'POST',
              headers: {
                'Authorization': `Bearer ${token}`
              },
              body: formData
            });
          }
        }

        if (res.ok) {
          const data = await res.json();
          if (data?.id) newFrase.id = data.id;
          if (data?.fileName) newFrase.arquivo = data.fileName;
        }
      }
    } catch (err) {
      console.warn('[API] Erro no upload de imagem de frase:', err);
    }

    const list = memoryCacheFrases || getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
    const updated = [newFrase, ...list.filter(item => item.id !== newFrase.id)];
    memoryCacheFrases = updated;
    setLocal(STORAGE_KEYS.FRASES, updated);
    return newFrase;
  },

  async updateFrase(id: string, data: { nomeHq: string; autor: string; arquivo?: string }): Promise<boolean> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        const payload = {
          id: id,
          nomeHQ: data.nomeHq.trim(),
          autor: data.autor.trim(),
          arquivo: data.arquivo?.trim() || ''
        };

        let res = await fetch(`${baseUrl}/frases/${id}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(payload)
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/frases/${id}`, {
              method: 'PUT',
              headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
              },
              body: JSON.stringify(payload)
            });
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao atualizar Frase na API:', err);
    }

    const list = memoryCacheFrases || getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
    const updated = list.map(item => {
      if (item.id === id) {
        return {
          ...item,
          nomeHQ: data.nomeHq.trim(),
          autor: data.autor.trim(),
          arquivo: data.arquivo?.trim() || item.arquivo
        };
      }
      return item;
    });
    memoryCacheFrases = updated;
    setLocal(STORAGE_KEYS.FRASES, updated);
    return true;
  },

  async removeFrase(id: string): Promise<boolean> {
    const list = memoryCacheFrases || getLocal<Frase[]>(STORAGE_KEYS.FRASES, []);
    const filtered = list.filter(item => item.id !== id);
    memoryCacheFrases = filtered;
    setLocal(STORAGE_KEYS.FRASES, filtered);

    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();
      if (token) {
        let res = await fetch(`${baseUrl}/frases/${id}`, {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${token}`
          }
        });

        if (res.status === 401) {
          setStoredToken(null);
          token = await ensureAuthenticated();
          if (token) {
            await fetch(`${baseUrl}/frases/${id}`, {
              method: 'DELETE',
              headers: {
                'Authorization': `Bearer ${token}`
              }
            });
          }
        }
      }
    } catch (err) {
      console.warn('[API] Erro ao excluir Frase na API:', err);
    }
    return true;
  },

  // ==========================================
  // IMPORTACAO LEGADO
  // ==========================================
  async getImportacoesLegado(): Promise<ImportacaoLegado[]> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/v1/ImportacaoLegado`, {
        headers: {
          'Accept': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        signal: AbortSignal.timeout(30000)
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/v1/ImportacaoLegado`, {
            headers: {
              'Accept': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            signal: AbortSignal.timeout(30000)
          });
        }
      }

      if (res.ok) {
        return await res.json();
      }
      return [];
    } catch (err) {
      console.warn('[API] Erro ao carregar Importacao Legado:', err);
      return [];
    }
  },

  async updateImportacaoLegado(id: string, data: { titulo: string; editora: string; edicao: number }): Promise<boolean> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        body: JSON.stringify(data)
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/v1/ImportacaoLegado/${id}`, {
            method: 'PUT',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(data)
          });
        }
      }

      return res.ok;
    } catch (err) {
      console.warn('[API] Erro ao atualizar item de Importação Legado:', err);
      return false;
    }
  },

  async deleteImportacaoLegado(id: string): Promise<boolean> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/${id}`, {
        method: 'DELETE',
        headers: {
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/v1/ImportacaoLegado/${id}`, {
            method: 'DELETE',
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
        }
      }

      return res.ok;
    } catch (err) {
      console.warn('[API] Erro ao excluir item de Importação Legado:', err);
      return false;
    }
  },

  async deleteMultipleImportacaoLegado(ids: string[]): Promise<boolean> {
    if (!ids || ids.length === 0) return true;
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/batch-delete`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        },
        body: JSON.stringify(ids)
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/v1/ImportacaoLegado/batch-delete`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify(ids)
          });
        }
      }

      if (res.ok) return true;

      // Fallback para exclusão sequencial se a rota em lote não responder
      const results = await Promise.all(ids.map(id => apiService.deleteImportacaoLegado(id)));
      return results.some(r => r);
    } catch (err) {
      console.warn('[API] Erro ao excluir lote de itens de Importação Legado, usando fallback:', err);
      const results = await Promise.all(ids.map(id => apiService.deleteImportacaoLegado(id)));
      return results.some(r => r);
    }
  },

  async clearAllImportacoesLegado(): Promise<boolean> {
    try {
      const baseUrl = getStoredApiUrl();
      let token = await ensureAuthenticated();

      let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/limpar-tudo`, {
        method: 'DELETE',
        headers: {
          ...(token ? { 'Authorization': `Bearer ${token}` } : {})
        }
      });

      if (res.status === 401) {
        setStoredToken(null);
        token = await ensureAuthenticated();
        if (token) {
          res = await fetch(`${baseUrl}/v1/ImportacaoLegado/limpar-tudo`, {
            method: 'DELETE',
            headers: {
              'Authorization': `Bearer ${token}`
            }
          });
        }
      }

      return res.ok;
    } catch (err) {
      console.warn('[API] Erro ao limpar fila de Importação Legado:', err);
      return false;
    }
  },

  async uploadJsonLegado(file: File): Promise<ImportacaoLegadoResult> {
    const baseUrl = getStoredApiUrl();
    let token = await ensureAuthenticated();

    const formData = new FormData();
    formData.append('file', file);

    let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/upload-json`, {
      method: 'POST',
      headers: {
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
      },
      body: formData
    });

    if (res.status === 401) {
      setStoredToken(null);
      token = await ensureAuthenticated();
      if (token) {
        res = await fetch(`${baseUrl}/v1/ImportacaoLegado/upload-json`, {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`
          },
          body: formData
        });
      }
    }

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(errorText || 'Falha ao processar o arquivo JSON.');
    }

    return await res.json();
  },

  async processarJsonLegado(items: any[]): Promise<ImportacaoLegadoResult> {
    const baseUrl = getStoredApiUrl();
    let token = await ensureAuthenticated();

    let res = await fetch(`${baseUrl}/v1/ImportacaoLegado/processar-json`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
      },
      body: JSON.stringify(items)
    });

    if (res.status === 401) {
      setStoredToken(null);
      token = await ensureAuthenticated();
      if (token) {
        res = await fetch(`${baseUrl}/v1/ImportacaoLegado/processar-json`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(items)
        });
      }
    }

    if (!res.ok) {
      const errorText = await res.text();
      throw new Error(errorText || 'Falha ao processar o JSON.');
    }

    return await res.json();
  }
};
