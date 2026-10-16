import React, { useState, useEffect, useRef } from 'react';
import { 
  Upload, 
  Trash2, 
  Search, 
  Globe, 
  Edit2, 
  Check, 
  X, 
  RefreshCw, 
  FileCode, 
  AlertCircle, 
  CheckCircle2, 
  Loader2,
  BookOpen,
  Filter,
  Layers,
  Sparkles
} from 'lucide-react';
import { ImportacaoLegado, ImportacaoLegadoResult, HQ, Editora } from '../types';
import { apiService, getCapaUrl } from '../services/api';
import { AddHQModal } from '../components/AddHQModal';

interface LegacyImportPageProps {
  editoras?: Editora[];
  onHQAdded?: (hq: Omit<HQ, 'id'>, coverFile?: File) => Promise<void> | void;
}

interface ColecaoMatch {
  hq: HQ;
  matchType: 'exato' | 'mesmo_titulo_editora' | 'mesmo_titulo' | 'semelhante';
  score: number;
  motivo: string;
}

function normalizeStr(s?: string | null): string {
  if (!s) return '';
  return s
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/[^\w\s]/gi, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

function calculateSimilarity(
  queryTitulo: string,
  queryEditora: string,
  queryEdicao: number,
  hqList: HQ[]
): ColecaoMatch[] {
  const normTit = normalizeStr(queryTitulo);
  const normEd = normalizeStr(queryEditora);
  const numEd = queryEdicao <= 0 ? 1 : queryEdicao;

  if (!normTit) return [];

  const titWords = new Set(normTit.split(' ').filter(w => w.length > 2));
  const results: ColecaoMatch[] = [];

  for (const hq of hqList) {
    const hqTit = normalizeStr(hq.titulo);
    const hqEd = normalizeStr(hq.editora);
    const hqNum = (hq.numeroEdicao || 1) <= 0 ? 1 : (hq.numeroEdicao || 1);

    if (!hqTit) continue;

    const isExactTitle = normTit === hqTit;
    const isEditoraMatch = normEd && hqEd && (normEd === hqEd || normEd.includes(hqEd) || hqEd.includes(normEd));
    const isEditionMatch = numEd === hqNum;

    // 1. Correspondência Exata (Título, Editora e Edição)
    if (isExactTitle && isEditoraMatch && isEditionMatch) {
      results.push({
        hq,
        matchType: 'exato',
        score: 100,
        motivo: 'Correspondência Exata: Título, Editora e Edição coincidem perfeitamente!'
      });
      continue;
    }

    // 2. Mesmo Título e Mesma Editora (Edição diferente)
    if (isExactTitle && isEditoraMatch) {
      results.push({
        hq,
        matchType: 'mesmo_titulo_editora',
        score: 85,
        motivo: `Mesmo Título e Editora, porém edição #${hqNum}`
      });
      continue;
    }

    // 3. Mesmo Título (Editora ou Edição diferente)
    if (isExactTitle) {
      results.push({
        hq,
        matchType: 'mesmo_titulo',
        score: 75,
        motivo: `Mesmo Título (Editora: ${hq.editora || '—'}, Edição #${hqNum})`
      });
      continue;
    }

    // 4. Título Semelhante (Contém ou palavras em comum)
    const containsTitle = normTit.includes(hqTit) || hqTit.includes(normTit);
    const hqWords = hqTit.split(' ').filter(w => w.length > 2);
    let commonWordCount = 0;
    for (const w of hqWords) {
      if (titWords.has(w)) commonWordCount++;
    }

    const wordRatio = titWords.size > 0 ? commonWordCount / titWords.size : 0;

    if (containsTitle || wordRatio >= 0.5) {
      const score = Math.round(50 + (containsTitle ? 15 : 0) + (wordRatio * 15) + (isEditionMatch ? 5 : 0) + (isEditoraMatch ? 5 : 0));
      results.push({
        hq,
        matchType: 'semelhante',
        score,
        motivo: `Título semelhante (${Math.round(wordRatio * 100)}% das palavras-chave em comum)`
      });
    }
  }

  results.sort((a, b) => b.score - a.score);
  return results.slice(0, 30);
}

export const LegacyImportPage: React.FC<LegacyImportPageProps> = ({ editoras: initialEditoras, onHQAdded }) => {
  const [items, setItems] = useState<ImportacaoLegado[]>([]);
  const [editoras, setEditoras] = useState<Editora[]>(initialEditoras || []);
  const [isLoading, setIsLoading] = useState(true);
  const [isUploading, setIsUploading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Multi-selection state
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [isDeletingSelected, setIsDeletingSelected] = useState(false);

  // Feedback / Alert state
  const [notification, setNotification] = useState<{
    type: 'success' | 'error' | 'info';
    message: string;
  } | null>(null);

  // In-line editing state
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editTitulo, setEditTitulo] = useState('');
  const [editEditora, setEditEditora] = useState('');
  const [editEdicao, setEditEdicao] = useState<number>(1);
  const [isSavingEdit, setIsSavingEdit] = useState(false);

  // Modal for "Buscar na Web" (Guia dos Quadrinhos)
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedWebItem, setSelectedWebItem] = useState<ImportacaoLegado | null>(null);

  // Modal for "Buscar na Coleção"
  const [isColecaoModalOpen, setIsColecaoModalOpen] = useState(false);
  const [activeColecaoItem, setActiveColecaoItem] = useState<ImportacaoLegado | null>(null);
  const [colecaoSearchTitulo, setColecaoSearchTitulo] = useState('');
  const [colecaoSearchEditora, setColecaoSearchEditora] = useState('');
  const [colecaoSearchEdicao, setColecaoSearchEdicao] = useState<number>(1);
  const [colecaoMatches, setColecaoMatches] = useState<ColecaoMatch[]>([]);
  const [isSearchingColecao, setIsSearchingColecao] = useState(false);
  const [allColecaoHQs, setAllColecaoHQs] = useState<HQ[]>([]);

  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    loadItems();
    if (!initialEditoras || initialEditoras.length === 0) {
      apiService.getEditoras().then(eds => setEditoras(eds)).catch(() => {});
    }
  }, []);

  const showNotification = (type: 'success' | 'error' | 'info', message: string) => {
    setNotification({ type, message });
    setTimeout(() => {
      setNotification(prev => (prev?.message === message ? null : prev));
    }, 6000);
  };

  const loadColecao = async (): Promise<HQ[]> => {
    if (allColecaoHQs.length > 0) return allColecaoHQs;
    try {
      const data = await apiService.getHQs();
      setAllColecaoHQs(data);
      return data;
    } catch {
      return [];
    }
  };

  const loadItems = async () => {
    setIsLoading(true);
    try {
      const data = await apiService.getImportacoesLegado();
      setItems(data);
      setSelectedIds(new Set());
    } catch (err: any) {
      showNotification('error', 'Erro ao carregar registros de importação legado: ' + err.message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.name.toLowerCase().endsWith('.json')) {
      showNotification('error', 'Por favor, selecione um arquivo no formato .json exportado do SQLite.');
      if (fileInputRef.current) fileInputRef.current.value = '';
      return;
    }

    setIsUploading(true);
    try {
      const result: ImportacaoLegadoResult = await apiService.uploadJsonLegado(file);
      showNotification(
        'success', 
        result.mensagem || `Importação concluída: ${result.novosImportados} novos registros adicionados à fila.`
      );
      await loadItems();
    } catch (err: any) {
      showNotification('error', 'Erro ao processar arquivo: ' + err.message);
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  // Inline edit handlers
  const startEdit = (item: ImportacaoLegado) => {
    setEditingId(item.id);
    setEditTitulo(item.titulo);
    setEditEditora(item.editora);
    setEditEdicao(item.edicao);
  };

  const cancelEdit = () => {
    setEditingId(null);
  };

  const saveEdit = async (id: string) => {
    if (!editTitulo.trim()) {
      alert('O título não pode estar vazio.');
      return;
    }

    setIsSavingEdit(true);
    try {
      const ok = await apiService.updateImportacaoLegado(id, {
        titulo: editTitulo.trim(),
        editora: editEditora.trim(),
        edicao: editEdicao <= 0 ? 1 : editEdicao
      });

      if (ok) {
        setItems(prev => prev.map(item => {
          if (item.id === id) {
            return {
              ...item,
              titulo: editTitulo.trim(),
              editora: editEditora.trim(),
              edicao: editEdicao <= 0 ? 1 : editEdicao
            };
          }
          return item;
        }));
        setEditingId(null);
        showNotification('success', 'Registro atualizado com sucesso.');
      } else {
        showNotification('error', 'Não foi possível atualizar o registro.');
      }
    } catch (err: any) {
      showNotification('error', 'Erro ao atualizar: ' + err.message);
    } finally {
      setIsSavingEdit(false);
    }
  };

  // Single delete
  const handleDelete = async (id: string, titulo: string) => {
    if (!window.confirm(`Tem certeza que deseja excluir o registro "${titulo}" da fila?`)) {
      return;
    }

    try {
      const ok = await apiService.deleteImportacaoLegado(id);
      if (ok) {
        setItems(prev => prev.filter(item => item.id !== id));
        setSelectedIds(prev => {
          const next = new Set(prev);
          next.delete(id);
          return next;
        });
        showNotification('success', `Registro "${titulo}" removido.`);
      } else {
        showNotification('error', 'Erro ao remover registro.');
      }
    } catch (err: any) {
      showNotification('error', 'Erro: ' + err.message);
    }
  };

  // Multi-selection delete
  const toggleSelect = (id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleDeleteSelected = async () => {
    const count = selectedIds.size;
    if (count === 0) return;

    if (!window.confirm(`Tem certeza que deseja excluir ${count} quadrinho(s) selecionado(s) de uma só vez da fila de importação legado?`)) {
      return;
    }

    setIsDeletingSelected(true);
    try {
      const idsArray = Array.from(selectedIds);
      const ok = await apiService.deleteMultipleImportacaoLegado(idsArray);
      if (ok) {
        setItems(prev => prev.filter(item => !selectedIds.has(item.id)));
        setSelectedIds(new Set());
        showNotification('success', `${count} quadrinho(s) excluído(s) com sucesso da fila!`);
      } else {
        showNotification('error', 'Erro ao excluir quadrinhos selecionados.');
      }
    } catch (err: any) {
      showNotification('error', 'Erro: ' + err.message);
    } finally {
      setIsDeletingSelected(false);
    }
  };

  const handleClearAll = async () => {
    if (items.length === 0) return;
    if (!window.confirm('Tem certeza que deseja LIMPAR TODOS os registros da fila de Importação Legado?')) {
      return;
    }

    try {
      const ok = await apiService.clearAllImportacoesLegado();
      if (ok) {
        setItems([]);
        setSelectedIds(new Set());
        showNotification('success', 'Fila de importação legado foi esvaziada.');
      } else {
        showNotification('error', 'Erro ao esvaziar a fila.');
      }
    } catch (err: any) {
      showNotification('error', 'Erro: ' + err.message);
    }
  };

  // Web search modal
  const handleOpenWebSearch = (item: ImportacaoLegado) => {
    setSelectedWebItem(item);
    setIsModalOpen(true);
  };

  const handleSuccessImport = async (_importedHQ?: HQ) => {
    if (!selectedWebItem) return;
    
    try {
      await apiService.deleteImportacaoLegado(selectedWebItem.id);
      setItems(prev => prev.filter(item => item.id !== selectedWebItem.id));
      setSelectedIds(prev => {
        const next = new Set(prev);
        next.delete(selectedWebItem.id);
        return next;
      });
      showNotification(
        'success', 
        `HQ "${selectedWebItem.titulo}" importada com sucesso para a coleção e removida da fila de importação legado!`
      );
    } catch (err) {
      console.warn('Erro ao remover item após importação:', err);
    }
    setSelectedWebItem(null);
  };

  // Buscar na Coleção
  const handleOpenColecaoSearch = async (item?: ImportacaoLegado) => {
    const targetItem = item || (selectedIds.size === 1 ? items.find(i => selectedIds.has(i.id)) : null) || filteredItems[0] || null;
    setActiveColecaoItem(targetItem);

    const tit = targetItem ? targetItem.titulo : (searchTerm.trim() || '');
    const ed = targetItem ? targetItem.editora : '';
    const num = targetItem ? targetItem.edicao : 1;

    setColecaoSearchTitulo(tit);
    setColecaoSearchEditora(ed);
    setColecaoSearchEdicao(num);
    setIsColecaoModalOpen(true);

    if (tit) {
      setIsSearchingColecao(true);
      const hqs = await loadColecao();
      const matches = calculateSimilarity(tit, ed, num, hqs);
      setColecaoMatches(matches);
      setIsSearchingColecao(false);
    } else {
      setColecaoMatches([]);
    }
  };

  const handlePerformColecaoSearch = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!colecaoSearchTitulo.trim()) return;

    setIsSearchingColecao(true);
    const hqs = await loadColecao();
    const matches = calculateSimilarity(colecaoSearchTitulo, colecaoSearchEditora, colecaoSearchEdicao, hqs);
    setColecaoMatches(matches);
    setIsSearchingColecao(false);
  };

  const handleRemoveActiveItemFromQueue = async () => {
    if (!activeColecaoItem) return;
    if (!window.confirm(`Remover "${activeColecaoItem.titulo}" da fila de importação legado?`)) return;

    try {
      const ok = await apiService.deleteImportacaoLegado(activeColecaoItem.id);
      if (ok) {
        setItems(prev => prev.filter(i => i.id !== activeColecaoItem.id));
        setSelectedIds(prev => {
          const next = new Set(prev);
          next.delete(activeColecaoItem.id);
          return next;
        });
        showNotification('success', `"${activeColecaoItem.titulo}" removido da fila de importação.`);
        setIsColecaoModalOpen(false);
        setActiveColecaoItem(null);
      }
    } catch (err: any) {
      showNotification('error', 'Erro ao remover item: ' + err.message);
    }
  };

  const handleSwitchToWeb = () => {
    if (activeColecaoItem) {
      const itemToSearch = activeColecaoItem;
      setIsColecaoModalOpen(false);
      handleOpenWebSearch(itemToSearch);
    }
  };

  const filteredItems = items.filter(item => {
    if (!searchTerm.trim()) return true;
    const term = searchTerm.toLowerCase();
    return (
      item.titulo.toLowerCase().includes(term) ||
      item.editora.toLowerCase().includes(term) ||
      String(item.edicao).includes(term)
    );
  });

  const isAllVisibleSelected = filteredItems.length > 0 && filteredItems.every(i => selectedIds.has(i.id));

  const toggleSelectAllVisible = () => {
    if (isAllVisibleSelected) {
      setSelectedIds(prev => {
        const next = new Set(prev);
        filteredItems.forEach(i => next.delete(i.id));
        return next;
      });
    } else {
      setSelectedIds(prev => {
        const next = new Set(prev);
        filteredItems.forEach(i => next.add(i.id));
        return next;
      });
    }
  };

  return (
    <div style={{ padding: '24px 20px', maxWidth: 1200, margin: '0 auto', display: 'flex', flexDirection: 'column', gap: 20 }}>
      {/* Header */}
      <div style={{
        display: 'flex',
        flexWrap: 'wrap',
        justifyContent: 'space-between',
        alignItems: 'center',
        gap: 16,
        paddingBottom: 16,
        borderBottom: '1px solid var(--border-subtle)'
      }}>
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 4 }}>
            <span style={{
              display: 'inline-flex',
              padding: '4px 10px',
              borderRadius: 'var(--radius-full)',
              background: 'rgba(99, 102, 241, 0.15)',
              color: 'var(--primary)',
              fontSize: '0.78rem',
              fontWeight: 700,
              textTransform: 'uppercase',
              letterSpacing: '0.05em'
            }}>
              Módulo de Migração
            </span>
          </div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 800, margin: 0, color: '#fff' }}>
            Importação de Legado
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4, maxWidth: 650 }}>
            Gerencie os quadrinhos importados do banco SQLite antigo. Compare títulos com sua <strong>Coleção</strong> ou pesquise no <strong>Guia dos Quadrinhos</strong>.
          </p>
        </div>

        {/* Action Buttons Top */}
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileUpload}
            accept=".json"
            style={{ display: 'none' }}
          />

          <button
            onClick={() => fileInputRef.current?.click()}
            disabled={isUploading}
            className="btn-primary"
            style={{
              padding: '10px 18px',
              fontSize: '0.9rem',
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              background: 'linear-gradient(135deg, var(--primary), var(--accent-pink))'
            }}
          >
            {isUploading ? <Loader2 size={18} className="animate-spin" /> : <Upload size={18} />}
            <span>Upload SQLLite</span>
          </button>

          {/* BOTÃO GLOBAL: Buscar na Coleção */}
          <button
            onClick={() => handleOpenColecaoSearch()}
            className="btn-secondary"
            style={{
              padding: '10px 16px',
              fontSize: '0.9rem',
              display: 'flex',
              alignItems: 'center',
              gap: 8,
              border: '1px solid rgba(99, 102, 241, 0.4)',
              color: '#818cf8',
              background: 'rgba(99, 102, 241, 0.08)'
            }}
            title="Bater Título, Editora e Edição com os registros da Coleção"
          >
            <BookOpen size={18} />
            <span>Buscar na Coleção</span>
          </button>

          <button
            onClick={loadItems}
            disabled={isLoading}
            className="btn-secondary"
            style={{ padding: '10px 14px', fontSize: '0.9rem', display: 'flex', alignItems: 'center', gap: 6 }}
            title="Recarregar registros"
          >
            <RefreshCw size={16} className={isLoading ? 'animate-spin' : ''} />
            <span>Recarregar</span>
          </button>

          {items.length > 0 && (
            <button
              onClick={handleClearAll}
              className="btn-secondary"
              style={{
                padding: '10px 14px',
                fontSize: '0.9rem',
                display: 'flex',
                alignItems: 'center',
                gap: 6,
                color: 'var(--accent-rose)'
              }}
              title="Limpar todos os registros pendentes"
            >
              <Trash2 size={16} />
              <span>Limpar Fila</span>
            </button>
          )}
        </div>
      </div>

      {/* Notifications */}
      {notification && (
        <div style={{
          padding: '14px 18px',
          borderRadius: 'var(--radius-md)',
          display: 'flex',
          alignItems: 'center',
          gap: 12,
          fontSize: '0.9rem',
          fontWeight: 600,
          background: notification.type === 'success' 
            ? 'rgba(16, 185, 129, 0.15)' 
            : notification.type === 'error'
              ? 'rgba(244, 63, 94, 0.15)'
              : 'rgba(99, 102, 241, 0.15)',
          color: notification.type === 'success' 
            ? 'var(--accent-emerald)' 
            : notification.type === 'error'
              ? 'var(--accent-rose)'
              : 'var(--accent-cyan)',
          border: `1px solid ${
            notification.type === 'success' 
              ? 'rgba(16, 185, 129, 0.3)' 
              : notification.type === 'error'
                ? 'rgba(244, 63, 94, 0.3)'
                : 'rgba(99, 102, 241, 0.3)'
          }`
        }}>
          {notification.type === 'success' && <CheckCircle2 size={20} />}
          {notification.type === 'error' && <AlertCircle size={20} />}
          {notification.type === 'info' && <AlertCircle size={20} />}
          <span style={{ flex: 1 }}>{notification.message}</span>
          <button 
            onClick={() => setNotification(null)}
            style={{ background: 'none', border: 'none', color: 'inherit', cursor: 'pointer', padding: 4 }}
          >
            <X size={16} />
          </button>
        </div>
      )}

      {/* Multi-selection Action Banner */}
      {selectedIds.size > 0 && (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '12px 20px',
          background: 'linear-gradient(90deg, rgba(99, 102, 241, 0.15), rgba(244, 63, 94, 0.15))',
          border: '1px solid rgba(99, 102, 241, 0.35)',
          borderRadius: 'var(--radius-md)',
          gap: 14,
          flexWrap: 'wrap',
          boxShadow: '0 4px 16px rgba(0,0,0,0.2)'
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{
              width: 32,
              height: 32,
              borderRadius: '50%',
              background: 'var(--primary)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: 800,
              fontSize: '0.88rem',
              color: '#fff'
            }}>
              {selectedIds.size}
            </div>
            <div>
              <span style={{ fontSize: '0.95rem', color: '#fff', fontWeight: 700 }}>
                {selectedIds.size} {selectedIds.size === 1 ? 'quadrinho selecionado' : 'quadrinhos selecionados'}
              </span>
              <span style={{ display: 'block', fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                Selecione as ações em lote desejadas abaixo:
              </span>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
            {selectedIds.size === 1 && (
              <button
                onClick={() => handleOpenColecaoSearch()}
                className="btn-secondary"
                style={{
                  padding: '7px 14px',
                  fontSize: '0.84rem',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 6,
                  color: '#818cf8',
                  background: 'rgba(99, 102, 241, 0.15)'
                }}
              >
                <BookOpen size={14} />
                <span>Buscar na Coleção</span>
              </button>
            )}

            <button
              onClick={handleDeleteSelected}
              disabled={isDeletingSelected}
              className="btn-primary"
              style={{
                background: 'linear-gradient(135deg, var(--accent-rose), #dc2626)',
                boxShadow: '0 2px 10px rgba(244, 63, 94, 0.4)',
                padding: '8px 18px',
                fontSize: '0.88rem',
                display: 'flex',
                alignItems: 'center',
                gap: 8,
                fontWeight: 700
              }}
            >
              {isDeletingSelected ? <Loader2 size={16} className="animate-spin" /> : <Trash2 size={16} />}
              <span>Excluir Selecionadas ({selectedIds.size})</span>
            </button>

            <button
              onClick={() => setSelectedIds(new Set())}
              className="btn-secondary"
              style={{ padding: '8px 14px', fontSize: '0.85rem' }}
            >
              Desmarcar Todas
            </button>
          </div>
        </div>
      )}

      {/* Stats and Filter Bar */}
      <div style={{
        display: 'flex',
        flexWrap: 'wrap',
        justifyContent: 'space-between',
        alignItems: 'center',
        gap: 12
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10, flexWrap: 'wrap' }}>
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: 8,
            padding: '6px 14px',
            background: 'var(--bg-card)',
            borderRadius: 'var(--radius-full)',
            border: '1px solid var(--border-subtle)',
            fontSize: '0.85rem'
          }}>
            <BookOpen size={16} color="var(--primary)" />
            <span>Pendentes na Fila:</span>
            <strong style={{ color: 'var(--accent-cyan)', fontSize: '0.95rem' }}>{items.length}</strong>
          </div>

          {filteredItems.length !== items.length && (
            <div style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 6,
              padding: '6px 12px',
              background: 'rgba(255,255,255,0.04)',
              borderRadius: 'var(--radius-full)',
              fontSize: '0.82rem',
              color: 'var(--text-muted)'
            }}>
              <Filter size={14} />
              <span>Filtrados: <strong>{filteredItems.length}</strong></span>
            </div>
          )}
        </div>

        {/* Filter Input */}
        <div style={{ position: 'relative', minWidth: 260 }}>
          <Search size={16} style={{ position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input
            type="text"
            placeholder="Filtrar por título, editora..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '9px 12px 9px 36px',
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-sm)',
              color: '#fff',
              fontSize: '0.88rem'
            }}
          />
          {searchTerm && (
            <button
              onClick={() => setSearchTerm('')}
              style={{
                position: 'absolute',
                right: 10,
                top: '50%',
                transform: 'translateY(-50%)',
                background: 'none',
                border: 'none',
                color: 'var(--text-muted)',
                cursor: 'pointer'
              }}
            >
              <X size={14} />
            </button>
          )}
        </div>
      </div>

      {/* Grid Table */}
      <div style={{
        background: 'var(--bg-card)',
        borderRadius: 'var(--radius-lg)',
        border: '1px solid var(--border-subtle)',
        overflow: 'hidden',
        boxShadow: '0 4px 20px rgba(0,0,0,0.15)'
      }}>
        {isLoading ? (
          <div style={{ padding: '60px 20px', textAlign: 'center', color: 'var(--text-muted)' }}>
            <Loader2 size={32} className="animate-spin" style={{ margin: '0 auto 12px' }} />
            <p>Carregando registros de importação...</p>
          </div>
        ) : filteredItems.length === 0 ? (
          <div style={{ padding: '60px 20px', textAlign: 'center', color: 'var(--text-muted)' }}>
            <FileCode size={40} style={{ margin: '0 auto 12px', opacity: 0.4 }} />
            <h3 style={{ fontSize: '1.1rem', color: '#fff', marginBottom: 6 }}>
              {items.length === 0 ? 'Nenhum quadrinho pendente' : 'Nenhum quadrinho encontrado com o filtro'}
            </h3>
            <p style={{ fontSize: '0.85rem', maxWidth: 450, margin: '0 auto' }}>
              {items.length === 0 
                ? 'Clique no botão "Upload SQLLite" acima e selecione o arquivo .json exportado do banco SQLite antigo.'
                : 'Tente alterar os termos da busca.'}
            </p>
          </div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.88rem' }}>
              <thead>
                <tr style={{ background: 'rgba(255,255,255,0.03)', borderBottom: '1px solid var(--border-subtle)' }}>
                  {/* Master Checkbox */}
                  <th style={{ padding: '14px 16px', width: 44, textAlign: 'center' }}>
                    <input
                      type="checkbox"
                      checked={isAllVisibleSelected}
                      onChange={toggleSelectAllVisible}
                      style={{ cursor: 'pointer', width: 16, height: 16, accentColor: 'var(--primary)' }}
                      title="Selecionar / Desmarcar todos os visíveis"
                    />
                  </th>
                  <th style={{ padding: '14px 12px', fontWeight: 700, color: 'var(--text-muted)', width: 50 }}>#</th>
                  <th style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--text-muted)' }}>TÍTULO</th>
                  <th style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--text-muted)' }}>EDITORA</th>
                  <th style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--text-muted)', width: 100 }}>EDIÇÃO</th>
                  <th style={{ padding: '14px 16px', fontWeight: 700, color: 'var(--text-muted)', textAlign: 'center', width: 340 }}>AÇÕES</th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.map((item, idx) => {
                  const isEditing = editingId === item.id;
                  const isSelected = selectedIds.has(item.id);

                  return (
                    <tr 
                      key={item.id}
                      style={{
                        borderBottom: '1px solid var(--border-subtle)',
                        background: isSelected 
                          ? 'rgba(99, 102, 241, 0.08)' 
                          : isEditing 
                            ? 'rgba(99, 102, 241, 0.05)' 
                            : 'transparent',
                        transition: 'background 0.15s ease'
                      }}
                    >
                      {/* Checkbox */}
                      <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => toggleSelect(item.id)}
                          style={{ cursor: 'pointer', width: 16, height: 16, accentColor: 'var(--primary)' }}
                        />
                      </td>

                      {/* Index */}
                      <td style={{ padding: '12px 12px', color: 'var(--text-muted)', fontSize: '0.8rem' }}>
                        {idx + 1}
                      </td>

                      {/* Titulo */}
                      <td style={{ padding: '12px 16px', fontWeight: 600 }}>
                        {isEditing ? (
                          <input
                            type="text"
                            value={editTitulo}
                            onChange={(e) => setEditTitulo(e.target.value)}
                            style={{
                              width: '100%',
                              padding: '6px 10px',
                              background: 'var(--bg-main)',
                              border: '1px solid var(--primary)',
                              borderRadius: 'var(--radius-sm)',
                              color: '#fff',
                              fontSize: '0.88rem'
                            }}
                          />
                        ) : (
                          <span style={{ color: '#fff' }}>{item.titulo}</span>
                        )}
                      </td>

                      {/* Editora */}
                      <td style={{ padding: '12px 16px' }}>
                        {isEditing ? (
                          <input
                            type="text"
                            value={editEditora}
                            onChange={(e) => setEditEditora(e.target.value)}
                            style={{
                              width: '100%',
                              padding: '6px 10px',
                              background: 'var(--bg-main)',
                              border: '1px solid var(--primary)',
                              borderRadius: 'var(--radius-sm)',
                              color: '#fff',
                              fontSize: '0.88rem'
                            }}
                          />
                        ) : (
                          <span style={{
                            display: 'inline-block',
                            padding: '3px 10px',
                            background: 'rgba(255,255,255,0.06)',
                            borderRadius: 'var(--radius-full)',
                            fontSize: '0.82rem',
                            color: 'var(--text-muted)'
                          }}>
                            {item.editora || '—'}
                          </span>
                        )}
                      </td>

                      {/* Edição */}
                      <td style={{ padding: '12px 16px' }}>
                        {isEditing ? (
                          <input
                            type="number"
                            min="1"
                            value={editEdicao}
                            onChange={(e) => setEditEdicao(parseInt(e.target.value, 10) || 1)}
                            style={{
                              width: '80px',
                              padding: '6px 10px',
                              background: 'var(--bg-main)',
                              border: '1px solid var(--primary)',
                              borderRadius: 'var(--radius-sm)',
                              color: '#fff',
                              fontSize: '0.88rem'
                            }}
                          />
                        ) : (
                          <span style={{
                            fontWeight: 700,
                            color: 'var(--accent-cyan)',
                            background: 'rgba(6, 182, 212, 0.1)',
                            padding: '3px 8px',
                            borderRadius: 6
                          }}>
                            Vol. {item.edicao}
                          </span>
                        )}
                      </td>

                      {/* Ações */}
                      <td style={{ padding: '12px 16px', textAlign: 'center' }}>
                        {isEditing ? (
                          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6 }}>
                            <button
                              onClick={() => saveEdit(item.id)}
                              disabled={isSavingEdit}
                              style={{
                                padding: '6px 10px',
                                background: 'var(--accent-emerald)',
                                border: 'none',
                                borderRadius: 'var(--radius-sm)',
                                color: '#fff',
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                                fontSize: '0.82rem'
                              }}
                              title="Salvar alterações"
                            >
                              <Check size={14} />
                              Salvar
                            </button>
                            <button
                              onClick={cancelEdit}
                              style={{
                                padding: '6px 10px',
                                background: 'rgba(255,255,255,0.1)',
                                border: 'none',
                                borderRadius: 'var(--radius-sm)',
                                color: 'var(--text-muted)',
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                                fontSize: '0.82rem'
                              }}
                              title="Cancelar edição"
                            >
                              <X size={14} />
                            </button>
                          </div>
                        ) : (
                          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 6, flexWrap: 'nowrap' }}>
                            {/* BOTÃO NOVO: Buscar na Coleção */}
                            <button
                              onClick={() => handleOpenColecaoSearch(item)}
                              className="btn-secondary"
                              style={{
                                padding: '6px 10px',
                                fontSize: '0.8rem',
                                display: 'flex',
                                alignItems: 'center',
                                gap: 5,
                                background: 'rgba(99, 102, 241, 0.12)',
                                border: '1px solid rgba(99, 102, 241, 0.3)',
                                color: '#818cf8',
                                cursor: 'pointer',
                                whiteSpace: 'nowrap'
                              }}
                              title={`Bater "${item.titulo}" na Coleção`}
                            >
                              <BookOpen size={13} />
                              <span>Buscar na Coleção</span>
                            </button>

                            {/* BOTÃO PRINCIPAL: Buscar na Web */}
                            <button
                              onClick={() => handleOpenWebSearch(item)}
                              className="btn-primary"
                              style={{
                                padding: '6px 10px',
                                fontSize: '0.8rem',
                                display: 'flex',
                                alignItems: 'center',
                                gap: 5,
                                background: 'linear-gradient(135deg, var(--accent-cyan), var(--primary))',
                                boxShadow: '0 2px 8px rgba(6, 182, 212, 0.3)',
                                whiteSpace: 'nowrap'
                              }}
                              title={`Buscar "${item.titulo}" no Guia dos Quadrinhos`}
                            >
                              <Globe size={13} />
                              <span>Buscar na Web</span>
                            </button>

                            {/* Editar */}
                            <button
                              onClick={() => startEdit(item)}
                              style={{
                                padding: '6px 8px',
                                background: 'rgba(255,255,255,0.06)',
                                border: '1px solid var(--border-subtle)',
                                borderRadius: 'var(--radius-sm)',
                                color: 'var(--text-muted)',
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center'
                              }}
                              title="Editar linha"
                            >
                              <Edit2 size={13} />
                            </button>

                            {/* Excluir Individual */}
                            <button
                              onClick={() => handleDelete(item.id, item.titulo)}
                              style={{
                                padding: '6px 8px',
                                background: 'rgba(244, 63, 94, 0.1)',
                                border: '1px solid rgba(244, 63, 94, 0.2)',
                                borderRadius: 'var(--radius-sm)',
                                color: 'var(--accent-rose)',
                                cursor: 'pointer',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center'
                              }}
                              title="Excluir da fila de importação"
                            >
                              <Trash2 size={13} />
                            </button>
                          </div>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* MODAL 1: BUSCAR NA COLEÇÃO */}
      {isColecaoModalOpen && (
        <div className="modal-overlay" onClick={() => setIsColecaoModalOpen(false)}>
          <div 
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 650, maxHeight: '90vh', overflowY: 'auto' }}
          >
            {/* Header */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 36,
                  height: 36,
                  borderRadius: 10,
                  background: 'linear-gradient(135deg, var(--primary), var(--accent-cyan))',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  boxShadow: '0 4px 12px rgba(99, 102, 241, 0.4)'
                }}>
                  <BookOpen size={20} color="#fff" />
                </div>
                <div>
                  <h3 style={{ fontSize: '1.15rem', fontWeight: 800, margin: 0, color: '#fff' }}>
                    Buscar na Coleção
                  </h3>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                    Bater Título, Editora e Edição com os quadrinhos da sua coleção local
                  </span>
                </div>
              </div>
              <button 
                onClick={() => setIsColecaoModalOpen(false)}
                style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}
              >
                <X size={20} />
              </button>
            </div>

            {/* Current Item Banner */}
            {activeColecaoItem && (
              <div style={{
                padding: '12px 14px',
                borderRadius: 'var(--radius-md)',
                background: 'rgba(255, 255, 255, 0.04)',
                border: '1px solid var(--border-subtle)',
                marginBottom: 16,
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                flexWrap: 'wrap',
                gap: 10
              }}>
                <div>
                  <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 700, display: 'block' }}>
                    Item da Fila de Importação:
                  </span>
                  <strong style={{ fontSize: '0.95rem', color: '#fff' }}>
                    {activeColecaoItem.titulo}
                  </strong>
                  <span style={{ fontSize: '0.82rem', color: 'var(--accent-cyan)', marginLeft: 8 }}>
                    {activeColecaoItem.editora ? `• ${activeColecaoItem.editora}` : ''} • Edição #{activeColecaoItem.edicao}
                  </span>
                </div>

                <div style={{ display: 'flex', gap: 8 }}>
                  <button
                    type="button"
                    onClick={handleRemoveActiveItemFromQueue}
                    className="btn-secondary"
                    style={{
                      padding: '6px 10px',
                      fontSize: '0.78rem',
                      color: 'var(--accent-rose)',
                      border: '1px solid rgba(244, 63, 94, 0.3)',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 4
                    }}
                    title="Excluir este item da fila de importação"
                  >
                    <Trash2 size={13} />
                    <span>Já tenho (Remover da fila)</span>
                  </button>

                  <button
                    type="button"
                    onClick={handleSwitchToWeb}
                    className="btn-primary"
                    style={{
                      padding: '6px 10px',
                      fontSize: '0.78rem',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 4,
                      background: 'linear-gradient(135deg, var(--accent-cyan), var(--primary))'
                    }}
                    title="Buscar este quadrinho na Web (Guia dos Quadrinhos)"
                  >
                    <Globe size={13} />
                    <span>Buscar na Web</span>
                  </button>
                </div>
              </div>
            )}

            {/* Search Input Controls */}
            <form onSubmit={handlePerformColecaoSearch} style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 18 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr 0.6fr', gap: 8 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.74rem', fontWeight: 700, marginBottom: 3, color: 'var(--text-muted)' }}>
                    TÍTULO
                  </label>
                  <input
                    type="text"
                    value={colecaoSearchTitulo}
                    onChange={(e) => setColecaoSearchTitulo(e.target.value)}
                    placeholder="Título da HQ..."
                    style={{
                      width: '100%',
                      padding: '8px 10px',
                      background: 'var(--bg-main)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.85rem'
                    }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.74rem', fontWeight: 700, marginBottom: 3, color: 'var(--text-muted)' }}>
                    EDITORA
                  </label>
                  <input
                    type="text"
                    value={colecaoSearchEditora}
                    onChange={(e) => setColecaoSearchEditora(e.target.value)}
                    placeholder="Ex: Panini, Abril..."
                    style={{
                      width: '100%',
                      padding: '8px 10px',
                      background: 'var(--bg-main)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.85rem'
                    }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.74rem', fontWeight: 700, marginBottom: 3, color: 'var(--text-muted)' }}>
                    EDIÇÃO
                  </label>
                  <input
                    type="number"
                    min="1"
                    value={colecaoSearchEdicao}
                    onChange={(e) => setColecaoSearchEdicao(parseInt(e.target.value, 10) || 1)}
                    style={{
                      width: '100%',
                      padding: '8px 10px',
                      background: 'var(--bg-main)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.85rem'
                    }}
                  />
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
                <button
                  type="submit"
                  disabled={isSearchingColecao || !colecaoSearchTitulo.trim()}
                  className="btn-primary"
                  style={{
                    padding: '8px 16px',
                    fontSize: '0.84rem',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 6
                  }}
                >
                  {isSearchingColecao ? <Loader2 size={15} className="animate-spin" /> : <Search size={15} />}
                  <span>Comparar com a Coleção</span>
                </button>
              </div>
            </form>

            {/* Results */}
            <div style={{ borderTop: '1px solid var(--border-subtle)', paddingTop: 14 }}>
              {isSearchingColecao ? (
                <div style={{ textAlign: 'center', padding: '36px 12px', color: 'var(--text-muted)' }}>
                  <Loader2 size={28} className="animate-spin" style={{ margin: '0 auto 8px', color: 'var(--primary)' }} />
                  <p style={{ fontSize: '0.88rem' }}>Analisando títulos semelhantes na Coleção...</p>
                </div>
              ) : colecaoMatches.length === 0 ? (
                <div style={{ textAlign: 'center', padding: '30px 16px', color: 'var(--text-muted)' }}>
                  <Layers size={36} style={{ margin: '0 auto 8px', opacity: 0.3 }} />
                  <h4 style={{ fontSize: '0.98rem', color: '#fff', marginBottom: 4 }}>
                    Nenhum quadrinho semelhante encontrado na Coleção
                  </h4>
                  <p style={{ fontSize: '0.82rem', maxWidth: 440, margin: '0 auto 12px' }}>
                    Não foram localizados registros com título similar em sua base cadastrada. Este item provavelmente ainda não faz parte da sua coleção.
                  </p>
                  {activeColecaoItem && (
                    <button
                      type="button"
                      onClick={handleSwitchToWeb}
                      className="btn-primary"
                      style={{
                        padding: '8px 16px',
                        fontSize: '0.84rem',
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: 6,
                        margin: '0 auto'
                      }}
                    >
                      <Globe size={15} />
                      <span>Buscar no Guia dos Quadrinhos (Web)</span>
                    </button>
                  )}
                </div>
              ) : (
                <div>
                  <div style={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    marginBottom: 12
                  }}>
                    <span style={{ fontSize: '0.82rem', fontWeight: 700, color: 'var(--text-muted)' }}>
                      ENCONTRADOS NA COLEÇÃO ({colecaoMatches.length}):
                    </span>
                    {colecaoMatches.some(m => m.matchType === 'exato') && (
                      <span style={{
                        fontSize: '0.75rem',
                        background: 'rgba(16, 185, 129, 0.15)',
                        color: 'var(--accent-emerald)',
                        padding: '3px 8px',
                        borderRadius: 6,
                        fontWeight: 700,
                        border: '1px solid rgba(16, 185, 129, 0.3)'
                      }}>
                        🎯 Correspondência Exata Detectada!
                      </span>
                    )}
                  </div>

                  <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                    {colecaoMatches.map((match, idx) => {
                      const hq = match.hq;
                      const coverSrc = hq.thumbCapa || getCapaUrl(hq.capa);

                      const badgeStyle = match.matchType === 'exato'
                        ? { bg: 'rgba(16, 185, 129, 0.15)', border: 'rgba(16, 185, 129, 0.3)', color: 'var(--accent-emerald)', label: 'Correspondência Exata' }
                        : match.matchType === 'mesmo_titulo_editora'
                          ? { bg: 'rgba(6, 182, 212, 0.15)', border: 'rgba(6, 182, 212, 0.3)', color: 'var(--accent-cyan)', label: 'Mesmo Título & Editora' }
                          : match.matchType === 'mesmo_titulo'
                            ? { bg: 'rgba(99, 102, 241, 0.15)', border: 'rgba(99, 102, 241, 0.3)', color: '#818cf8', label: 'Mesmo Título' }
                            : { bg: 'rgba(245, 158, 11, 0.15)', border: 'rgba(245, 158, 11, 0.3)', color: '#fbbf24', label: 'Título Semelhante' };

                      return (
                        <div
                          key={hq.id || idx}
                          style={{
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                            padding: '10px 12px',
                            borderRadius: 'var(--radius-sm)',
                            background: match.matchType === 'exato' ? 'rgba(16, 185, 129, 0.06)' : 'rgba(255, 255, 255, 0.02)',
                            border: `1px solid ${match.matchType === 'exato' ? 'rgba(16, 185, 129, 0.3)' : 'var(--border-subtle)'}`,
                            gap: 12
                          }}
                        >
                          <div style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
                            {/* Cover Thumbnail */}
                            <div style={{
                              width: 38,
                              height: 52,
                              borderRadius: 4,
                              background: 'var(--bg-main)',
                              overflow: 'hidden',
                              flexShrink: 0,
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              border: '1px solid var(--border-subtle)'
                            }}>
                              {coverSrc ? (
                                <img
                                  src={coverSrc}
                                  alt={hq.titulo}
                                  style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                                  onError={(e) => {
                                    (e.target as HTMLElement).style.display = 'none';
                                  }}
                                />
                              ) : (
                                <BookOpen size={16} color="var(--text-muted)" />
                              )}
                            </div>

                            {/* Info */}
                            <div style={{ minWidth: 0 }}>
                              <div style={{ display: 'flex', alignItems: 'center', gap: 6, flexWrap: 'wrap' }}>
                                <h5 style={{ fontSize: '0.88rem', fontWeight: 700, margin: 0, color: '#fff', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                  {hq.titulo}
                                </h5>
                                <span style={{
                                  fontSize: '0.7rem',
                                  padding: '1px 6px',
                                  borderRadius: 4,
                                  background: badgeStyle.bg,
                                  border: `1px solid ${badgeStyle.border}`,
                                  color: badgeStyle.color,
                                  fontWeight: 700
                                }}>
                                  {badgeStyle.label}
                                </span>
                              </div>

                              <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)', marginTop: 2 }}>
                                <span>{hq.editora || 'Independente'} • Edição #{hq.numeroEdicao ?? 1}</span>
                                {hq.dataPublicacao || hq.dataLancamento ? ` • ${hq.dataPublicacao || hq.dataLancamento}` : ''}
                                {hq.preco ? ` • ${hq.preco}` : ''}
                                {hq.lido === 1 ? ' • ✓ Lido' : ''}
                              </div>

                              <div style={{ fontSize: '0.72rem', color: badgeStyle.color, marginTop: 2, fontStyle: 'italic' }}>
                                {match.motivo}
                              </div>
                            </div>
                          </div>

                          {/* Action inside card */}
                          {activeColecaoItem && (
                            <button
                              type="button"
                              onClick={handleRemoveActiveItemFromQueue}
                              className="btn-secondary"
                              style={{
                                padding: '5px 9px',
                                fontSize: '0.75rem',
                                flexShrink: 0,
                                display: 'flex',
                                alignItems: 'center',
                                gap: 4,
                                color: 'var(--accent-rose)',
                                borderColor: 'rgba(244, 63, 94, 0.2)'
                              }}
                              title="Remover este item da fila de importação legado"
                            >
                              <Trash2 size={12} />
                              <span>Já tenho</span>
                            </button>
                          )}
                        </div>
                      );
                    })}
                  </div>
                </div>
              )}
            </div>

            {/* Footer */}
            <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 16 }}>
              <button
                type="button"
                className="btn-secondary"
                onClick={() => setIsColecaoModalOpen(false)}
                style={{ padding: '8px 16px', fontSize: '0.85rem' }}
              >
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL 2: AddHQModal acoplado para importar dados da web */}
      {isModalOpen && selectedWebItem && (
        <AddHQModal
          isOpen={isModalOpen}
          onClose={() => {
            setIsModalOpen(false);
            setSelectedWebItem(null);
          }}
          onAdd={async (hq, file) => {
            if (onHQAdded) {
              await onHQAdded(hq, file);
            } else {
              const created = await apiService.addHQ(hq);
              if (file && created?.id) {
                try {
                  await apiService.uploadCapa(created.id, file);
                } catch (err) {
                  console.warn('Erro ao enviar capa:', err);
                }
              }
            }
          }}
          editoras={editoras}
          initialTab="web"
          initialSearch={{
            titulo: selectedWebItem.titulo,
            editora: selectedWebItem.editora,
            edicao: selectedWebItem.edicao
          }}
          onSuccessImport={handleSuccessImport}
        />
      )}
    </div>
  );
};
