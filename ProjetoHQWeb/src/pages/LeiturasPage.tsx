import React, { useState, useEffect, useMemo } from 'react';
import { 
  CheckCircle2, 
  Award, 
  BookOpen, 
  Plus, 
  Search, 
  Trash2, 
  RefreshCw, 
  Calendar, 
  Check, 
  Clock, 
  X, 
  Edit3,
  BookmarkCheck,
  AlertCircle
} from 'lucide-react';
import { Leitura, HQ } from '../types';
import { apiService, getCapaUrl } from '../services/api';

export const LeiturasPage: React.FC = () => {
  const [leituras, setLeituras] = useState<Leitura[]>([]);
  const [hqs, setHqs] = useState<HQ[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState<'all' | 'read' | 'unread'>('all');

  // Modal states
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [editingLeitura, setEditingLeitura] = useState<Leitura | null>(null);
  const [deletingLeitura, setDeletingLeitura] = useState<Leitura | null>(null);

  // Form states for Add
  const [formTitulo, setFormTitulo] = useState('');
  const [formCapa, setFormCapa] = useState('');
  const [formDataPublicacao, setFormDataPublicacao] = useState('');
  const [formSelectedHqId, setFormSelectedHqId] = useState('');
  const [saving, setSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  // Form state for Edit status/date
  const [editLido, setEditLido] = useState<number>(0);
  const [editDataLeitura, setEditDataLeitura] = useState<string>('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async (forceRefresh = false) => {
    if (forceRefresh) setIsRefreshing(true);
    else setIsLoading(true);

    try {
      const [leiturasList, hqList] = await Promise.all([
        apiService.getLeituras(undefined, forceRefresh),
        apiService.getHQs(false).catch(() => [])
      ]);
      setLeituras(leiturasList);
      setHqs(hqList);
    } catch (err) {
      console.error('Erro ao carregar Leituras:', err);
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  };

  // Metrics
  const readList = useMemo(() => leituras.filter(l => l.lido === 1), [leituras]);
  const unreadList = useMemo(() => leituras.filter(l => l.lido === 0), [leituras]);
  const totalCount = leituras.length;
  const readCount = readList.length;
  const unreadCount = unreadList.length;
  const progressPercent = totalCount > 0 ? Math.round((readCount / totalCount) * 100) : 0;

  // Filtered leituras according to search and active tab
  const filteredLeituras = useMemo(() => {
    return leituras.filter(item => {
      const matchesSearch = item.titulo.toLowerCase().includes(searchTerm.toLowerCase().trim());
      if (!matchesSearch) return false;

      if (activeTab === 'read') return item.lido === 1;
      if (activeTab === 'unread') return item.lido === 0;
      return true;
    });
  }, [leituras, searchTerm, activeTab]);

  // Open Add Modal
  const handleOpenAdd = () => {
    setFormTitulo('');
    setFormCapa('');
    setFormDataPublicacao('');
    setFormSelectedHqId('');
    setErrorMessage('');
    setIsAddModalOpen(true);
  };

  // Selection from existing HQ collection to prefill Add form
  const handleSelectHq = (hqId: string) => {
    setFormSelectedHqId(hqId);
    const found = hqs.find(h => h.id === hqId);
    if (found) {
      setFormTitulo(found.titulo);
      setFormCapa(found.capa || '');
      setFormDataPublicacao(found.dataPublicacao || '');
    }
  };

  // Save new reading
  const handleSaveAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formTitulo.trim()) {
      setErrorMessage('O título da leitura é obrigatório.');
      return;
    }
    setSaving(true);
    setErrorMessage('');

    try {
      const created = await apiService.addLeitura({
        titulo: formTitulo.trim(),
        capa: formCapa.trim(),
        dataPublicacao: formDataPublicacao.trim()
      });

      setLeituras(prev => [created, ...prev.filter(l => l.id !== created.id)]);
      setIsAddModalOpen(false);
    } catch (err: any) {
      setErrorMessage(err.message || 'Erro ao adicionar leitura.');
    } finally {
      setSaving(false);
    }
  };

  // Quick toggle read status
  const handleQuickToggle = async (leitura: Leitura) => {
    const newLido = leitura.lido === 1 ? 0 : 1;
    const nowIso = new Date().toISOString();
    const newDataLeitura = newLido === 1 ? nowIso : null;

    // Optimistic UI update
    setLeituras(prev => prev.map(l => l.id === leitura.id ? { ...l, lido: newLido, dataLeitura: newDataLeitura } : l));

    try {
      await apiService.updateLeitura(leitura.id, newLido, newDataLeitura);
    } catch (err) {
      console.error('Erro ao atualizar status de leitura:', err);
      // Revert on failure
      loadData(true);
    }
  };

  // Open Edit Modal for custom date or title review
  const handleOpenEdit = (leitura: Leitura) => {
    setEditingLeitura(leitura);
    setEditLido(leitura.lido);
    if (leitura.dataLeitura) {
      try {
        const d = new Date(leitura.dataLeitura);
        setEditDataLeitura(d.toISOString().substring(0, 10));
      } catch {
        setEditDataLeitura('');
      }
    } else {
      setEditDataLeitura(new Date().toISOString().substring(0, 10));
    }
  };

  // Save Edit Modal
  const handleSaveEdit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingLeitura) return;
    setSaving(true);

    try {
      const resolvedDate = editLido === 1 ? (editDataLeitura ? new Date(editDataLeitura + 'T12:00:00Z').toISOString() : new Date().toISOString()) : null;
      await apiService.updateLeitura(editingLeitura.id, editLido, resolvedDate);

      setLeituras(prev => prev.map(l => l.id === editingLeitura.id ? {
        ...l,
        lido: editLido,
        dataLeitura: resolvedDate
      } : l));
      setEditingLeitura(null);
    } catch (err) {
      console.error('Erro ao atualizar leitura:', err);
    } finally {
      setSaving(false);
    }
  };

  // Confirm delete
  const handleConfirmDelete = async () => {
    if (!deletingLeitura) return;
    setSaving(true);
    try {
      await apiService.removeLeitura(deletingLeitura.id);
      setLeituras(prev => prev.filter(l => l.id !== deletingLeitura.id));
      setDeletingLeitura(null);
    } catch (err) {
      console.error('Erro ao excluir leitura:', err);
    } finally {
      setSaving(false);
    }
  };

  const formatDate = (dateStr?: string | null) => {
    if (!dateStr) return '';
    try {
      const d = new Date(dateStr);
      if (isNaN(d.getTime())) return dateStr;
      return d.toLocaleDateString('pt-BR');
    } catch {
      return dateStr;
    }
  };

  return (
    <div style={{ maxWidth: 1400, margin: '0 auto', paddingBottom: 60 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16, marginBottom: 24 }}>
        <div>
          <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--accent-emerald)', letterSpacing: 1, textTransform: 'uppercase' }}>
            Endpoint api/Leitura
          </span>
          <h1 style={{ fontSize: '2.1rem', fontWeight: 800, marginTop: 4, letterSpacing: '-0.5px' }}>
            Controle de Leituras
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.92rem', marginTop: 4 }}>
            Gerencie seus quadrinhos em andamento, fila de espera e histórico de edições concluídas.
          </p>
        </div>

        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <button
            onClick={() => loadData(true)}
            disabled={isRefreshing}
            className="btn-secondary"
            title="Recarregar dados do banco SQL Server"
            style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '10px 16px', borderRadius: 'var(--radius-sm)' }}
          >
            <RefreshCw size={17} className={isRefreshing ? 'animate-spin' : ''} />
            <span>{isRefreshing ? 'Atualizando...' : 'Recarregar'}</span>
          </button>

          <button
            onClick={handleOpenAdd}
            className="btn-primary"
            style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '10px 18px', borderRadius: 'var(--radius-sm)' }}
          >
            <Plus size={18} />
            <span>Nova Leitura</span>
          </button>
        </div>
      </div>

      {/* Progress & Metrics Dashboard */}
      <div style={{
        background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.12), rgba(15, 23, 42, 0.85))',
        border: '1px solid rgba(16, 185, 129, 0.3)',
        borderRadius: 'var(--radius-md)',
        padding: 24,
        marginBottom: 28,
        boxShadow: '0 10px 30px rgba(0, 0, 0, 0.35)',
        backdropFilter: 'blur(12px)'
      }}>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 20, alignItems: 'center' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div style={{
              width: 48,
              height: 48,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(16, 185, 129, 0.22)',
              border: '1px solid rgba(16, 185, 129, 0.4)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--accent-emerald)'
            }}>
              <Award size={26} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>Total Cadastrado</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: 'var(--text-main)', lineHeight: 1.2 }}>{totalCount}</div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div style={{
              width: 48,
              height: 48,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(16, 185, 129, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--accent-emerald)'
            }}>
              <CheckCircle2 size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>Concluídos</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: 'var(--accent-emerald)', lineHeight: 1.2 }}>{readCount}</div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div style={{
              width: 48,
              height: 48,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(245, 158, 11, 0.18)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#f59e0b'
            }}>
              <Clock size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>Na Fila de Leitura</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: '#f59e0b', lineHeight: 1.2 }}>{unreadCount}</div>
            </div>
          </div>

          <div style={{ textAlign: 'right' }}>
            <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 4 }}>Taxa de Conclusão</div>
            <div style={{ fontSize: '2.2rem', fontFamily: 'var(--font-display)', color: 'var(--accent-emerald)', fontWeight: 800, lineHeight: 1 }}>
              {progressPercent}%
            </div>
          </div>
        </div>

        {/* Progress Bar */}
        <div style={{
          width: '100%',
          height: 9,
          background: 'rgba(255, 255, 255, 0.1)',
          borderRadius: 9999,
          overflow: 'hidden',
          marginTop: 20
        }}>
          <div style={{
            width: `${progressPercent}%`,
            height: '100%',
            background: 'linear-gradient(90deg, #10b981 0%, #06b6d4 100%)',
            borderRadius: 9999,
            transition: 'width 0.5s ease-in-out'
          }} />
        </div>
      </div>

      {/* Search & Tabs Filter */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: 16,
        marginBottom: 24,
        background: 'var(--bg-card)',
        padding: '12px 18px',
        borderRadius: 'var(--radius-md)',
        border: '1px solid var(--border-subtle)'
      }}>
        {/* Tabs */}
        <div style={{ display: 'flex', gap: 6, background: 'rgba(0,0,0,0.25)', padding: 4, borderRadius: 'var(--radius-sm)' }}>
          <button
            onClick={() => setActiveTab('all')}
            style={{
              padding: '6px 14px',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.85rem',
              fontWeight: 600,
              border: 'none',
              cursor: 'pointer',
              background: activeTab === 'all' ? 'var(--primary)' : 'transparent',
              color: activeTab === 'all' ? '#fff' : 'var(--text-muted)',
              transition: 'all 0.2s ease'
            }}
          >
            Todas ({totalCount})
          </button>
          <button
            onClick={() => setActiveTab('read')}
            style={{
              padding: '6px 14px',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.85rem',
              fontWeight: 600,
              border: 'none',
              cursor: 'pointer',
              background: activeTab === 'read' ? 'var(--accent-emerald)' : 'transparent',
              color: activeTab === 'read' ? '#fff' : 'var(--text-muted)',
              transition: 'all 0.2s ease'
            }}
          >
            Lidas ({readCount})
          </button>
          <button
            onClick={() => setActiveTab('unread')}
            style={{
              padding: '6px 14px',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.85rem',
              fontWeight: 600,
              border: 'none',
              cursor: 'pointer',
              background: activeTab === 'unread' ? '#f59e0b' : 'transparent',
              color: activeTab === 'unread' ? '#fff' : 'var(--text-muted)',
              transition: 'all 0.2s ease'
            }}
          >
            Na Fila ({unreadCount})
          </button>
        </div>

        {/* Search */}
        <div style={{ position: 'relative', width: 320, maxWidth: '100%' }}>
          <Search size={17} style={{ position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input
            type="text"
            placeholder="Buscar por título..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '9px 12px 9px 38px',
              borderRadius: 'var(--radius-sm)',
              border: '1px solid var(--border-subtle)',
              background: 'var(--bg-main)',
              color: 'var(--text-main)',
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
                background: 'transparent',
                border: 'none',
                color: 'var(--text-muted)',
                cursor: 'pointer'
              }}
            >
              <X size={15} />
            </button>
          )}
        </div>
      </div>

      {/* Grid of Leituras */}
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: '80px 20px', color: 'var(--text-muted)' }}>
          <RefreshCw size={36} className="animate-spin" style={{ margin: '0 auto 12px', color: 'var(--primary)' }} />
          <div>Carregando leituras da base de dados...</div>
        </div>
      ) : filteredLeituras.length > 0 ? (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))',
          gap: 20
        }}>
          {filteredLeituras.map(item => (
            <LeituraCard
              key={item.id}
              leitura={item}
              formatDate={formatDate}
              onToggleRead={() => handleQuickToggle(item)}
              onEdit={() => handleOpenEdit(item)}
              onDelete={() => setDeletingLeitura(item)}
            />
          ))}
        </div>
      ) : (
        <div style={{
          textAlign: 'center',
          padding: '60px 20px',
          background: 'var(--bg-card)',
          borderRadius: 'var(--radius-md)',
          border: '1px dashed var(--border-subtle)'
        }}>
          <BookOpen size={44} color="var(--text-dim)" style={{ margin: '0 auto 14px' }} />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 700 }}>Nenhuma leitura encontrada</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            {searchTerm ? 'Nenhum resultado corresponde à sua pesquisa.' : 'Adicione itens à sua lista de leitura usando o botão "Nova Leitura".'}
          </p>
        </div>
      )}

      {/* Modal Adicionar Leitura */}
      {isAddModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsAddModalOpen(false)}>
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 520, width: '90%' }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 36,
                  height: 36,
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(99, 102, 241, 0.15)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--primary)'
                }}>
                  <Plus size={20} />
                </div>
                <h3 style={{ fontSize: '1.25rem', fontWeight: 700, margin: 0 }}>Adicionar à Leitura</h3>
              </div>
              <button
                onClick={() => setIsAddModalOpen(false)}
                className="btn-icon"
                style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)' }}
              >
                <X size={20} />
              </button>
            </div>

            {errorMessage && (
              <div style={{
                background: 'rgba(239, 68, 68, 0.15)',
                border: '1px solid rgba(239, 68, 68, 0.4)',
                borderRadius: 'var(--radius-sm)',
                padding: '10px 14px',
                marginBottom: 16,
                color: '#ef4444',
                fontSize: '0.85rem',
                display: 'flex',
                alignItems: 'center',
                gap: 8
              }}>
                <AlertCircle size={16} />
                <span>{errorMessage}</span>
              </div>
            )}

            <form onSubmit={handleSaveAdd}>
              {/* Optional HQ selector */}
              {hqs.length > 0 && (
                <div style={{ marginBottom: 16 }}>
                  <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                    Importar da Coleção (Opcional):
                  </label>
                  <select
                    value={formSelectedHqId}
                    onChange={(e) => handleSelectHq(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      borderRadius: 'var(--radius-sm)',
                      background: 'var(--bg-main)',
                      border: '1px solid var(--border-subtle)',
                      color: 'var(--text-main)',
                      fontSize: '0.9rem'
                    }}
                  >
                    <option value="">-- Selecione uma HQ da coleção ou digite abaixo --</option>
                    {hqs.slice(0, 150).map(h => (
                      <option key={h.id} value={h.id}>
                        {h.titulo} (Ed. {h.numeroEdicao}) - {h.editora}
                      </option>
                    ))}
                  </select>
                </div>
              )}

              {/* Titulo */}
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Título da Leitura *
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ex: Batman: O Cavaleiro das Trevas"
                  value={formTitulo}
                  onChange={(e) => setFormTitulo(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    borderRadius: 'var(--radius-sm)',
                    background: 'var(--bg-main)',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              {/* Data de Publicação */}
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Data / Ano de Publicação
                </label>
                <input
                  type="text"
                  placeholder="Ex: 2021 ou Outubro de 2020"
                  value={formDataPublicacao}
                  onChange={(e) => setFormDataPublicacao(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    borderRadius: 'var(--radius-sm)',
                    background: 'var(--bg-main)',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              {/* Capa */}
              <div style={{ marginBottom: 24 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Arquivo da Capa ou URL (Opcional)
                </label>
                <input
                  type="text"
                  placeholder="Ex: arquivo.jpg ou https://..."
                  value={formCapa}
                  onChange={(e) => setFormCapa(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    borderRadius: 'var(--radius-sm)',
                    background: 'var(--bg-main)',
                    border: '1px solid var(--border-subtle)',
                    color: 'var(--text-main)',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
                <button
                  type="button"
                  onClick={() => setIsAddModalOpen(false)}
                  className="btn-secondary"
                  style={{ padding: '9px 18px', borderRadius: 'var(--radius-sm)' }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="btn-primary"
                  style={{ padding: '9px 20px', borderRadius: 'var(--radius-sm)' }}
                >
                  {saving ? 'Salvando...' : 'Adicionar Leitura'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Editar Status / Data de Leitura */}
      {editingLeitura && (
        <div className="modal-backdrop" onClick={() => setEditingLeitura(null)}>
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 460, width: '90%' }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 18 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 36,
                  height: 36,
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(16, 185, 129, 0.15)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--accent-emerald)'
                }}>
                  <Edit3 size={18} />
                </div>
                <h3 style={{ fontSize: '1.2rem', fontWeight: 700, margin: 0 }}>Atualizar Leitura</h3>
              </div>
              <button
                onClick={() => setEditingLeitura(null)}
                className="btn-icon"
                style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)' }}
              >
                <X size={20} />
              </button>
            </div>

            <div style={{ marginBottom: 16, padding: '10px 14px', background: 'var(--bg-main)', borderRadius: 'var(--radius-sm)', border: '1px solid var(--border-subtle)' }}>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Título</div>
              <div style={{ fontWeight: 700, fontSize: '1rem', color: 'var(--text-main)', marginTop: 2 }}>{editingLeitura.titulo}</div>
            </div>

            <form onSubmit={handleSaveEdit}>
              {/* Status Radio */}
              <div style={{ marginBottom: 18 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 8 }}>
                  Status de Leitura
                </label>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10 }}>
                  <button
                    type="button"
                    onClick={() => setEditLido(1)}
                    style={{
                      padding: '12px 14px',
                      borderRadius: 'var(--radius-sm)',
                      border: editLido === 1 ? '2px solid var(--accent-emerald)' : '1px solid var(--border-subtle)',
                      background: editLido === 1 ? 'rgba(16, 185, 129, 0.15)' : 'var(--bg-main)',
                      color: editLido === 1 ? 'var(--accent-emerald)' : 'var(--text-muted)',
                      fontWeight: 700,
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      gap: 8
                    }}
                  >
                    <CheckCircle2 size={18} />
                    <span>Lido</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => setEditLido(0)}
                    style={{
                      padding: '12px 14px',
                      borderRadius: 'var(--radius-sm)',
                      border: editLido === 0 ? '2px solid #f59e0b' : '1px solid var(--border-subtle)',
                      background: editLido === 0 ? 'rgba(245, 158, 11, 0.15)' : 'var(--bg-main)',
                      color: editLido === 0 ? '#f59e0b' : 'var(--text-muted)',
                      fontWeight: 700,
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      gap: 8
                    }}
                  >
                    <Clock size={18} />
                    <span>Na Fila</span>
                  </button>
                </div>
              </div>

              {/* Data de leitura (apenas se lido) */}
              {editLido === 1 && (
                <div style={{ marginBottom: 22 }}>
                  <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                    Data da Leitura
                  </label>
                  <input
                    type="date"
                    value={editDataLeitura}
                    onChange={(e) => setEditDataLeitura(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      borderRadius: 'var(--radius-sm)',
                      background: 'var(--bg-main)',
                      border: '1px solid var(--border-subtle)',
                      color: 'var(--text-main)',
                      fontSize: '0.9rem'
                    }}
                  />
                </div>
              )}

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
                <button
                  type="button"
                  onClick={() => setEditingLeitura(null)}
                  className="btn-secondary"
                  style={{ padding: '9px 18px', borderRadius: 'var(--radius-sm)' }}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="btn-primary"
                  style={{ padding: '9px 20px', borderRadius: 'var(--radius-sm)' }}
                >
                  {saving ? 'Salvando...' : 'Salvar Alterações'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Confirmar Exclusão */}
      {deletingLeitura && (
        <div className="modal-backdrop" onClick={() => setDeletingLeitura(null)}>
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 420, width: '90%', textAlign: 'center', padding: '28px 24px' }}
          >
            <div style={{
              width: 54,
              height: 54,
              borderRadius: '50%',
              background: 'rgba(239, 68, 68, 0.15)',
              color: '#ef4444',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              margin: '0 auto 16px'
            }}>
              <Trash2 size={26} />
            </div>

            <h3 style={{ fontSize: '1.25rem', fontWeight: 800, marginBottom: 8 }}>
              Remover da Leitura?
            </h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: 20 }}>
              Tem certeza que deseja excluir <strong>"{deletingLeitura.titulo}"</strong> do seu controle de leitura? Esta ação removerá o registro do banco de dados.
            </p>

            <div style={{ display: 'flex', justifyContent: 'center', gap: 12 }}>
              <button
                type="button"
                onClick={() => setDeletingLeitura(null)}
                className="btn-secondary"
                style={{ padding: '10px 20px', borderRadius: 'var(--radius-sm)' }}
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={handleConfirmDelete}
                disabled={saving}
                style={{
                  padding: '10px 22px',
                  borderRadius: 'var(--radius-sm)',
                  background: '#ef4444',
                  color: '#fff',
                  border: 'none',
                  fontWeight: 700,
                  cursor: 'pointer'
                }}
              >
                {saving ? 'Excluindo...' : 'Excluir'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

// Card Component for individual Leitura
interface LeituraCardProps {
  leitura: Leitura;
  formatDate: (d?: string | null) => string;
  onToggleRead: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

const LeituraCard: React.FC<LeituraCardProps> = ({
  leitura,
  formatDate,
  onToggleRead,
  onEdit,
  onDelete
}) => {
  const [imgError, setImgError] = useState(false);
  const isRead = leitura.lido === 1;
  const coverUrl = getCapaUrl(leitura.capa);
  const showCover = coverUrl && !imgError;

  return (
    <div style={{
      background: 'var(--bg-card)',
      borderRadius: 'var(--radius-md)',
      border: isRead ? '1px solid rgba(16, 185, 129, 0.25)' : '1px solid var(--border-subtle)',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column',
      boxShadow: '0 4px 18px rgba(0,0,0,0.22)',
      transition: 'transform 0.2s ease, border-color 0.2s ease',
      position: 'relative'
    }}>
      {/* Cover / Banner Area */}
      <div style={{
        position: 'relative',
        height: 280,
        background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.95), rgba(15, 23, 42, 0.98))',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden'
      }}>
        {showCover ? (
          <img
            src={coverUrl}
            alt={leitura.titulo}
            onError={() => setImgError(true)}
            style={{
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              transition: 'transform 0.3s ease'
            }}
          />
        ) : (
          <div style={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 10,
            padding: 16,
            textAlign: 'center'
          }}>
            <div style={{
              width: 52,
              height: 52,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(99, 102, 241, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)'
            }}>
              <BookOpen size={26} />
            </div>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 600 }}>
              Sem Imagem de Capa
            </span>
          </div>
        )}

        {/* Status Badge Tag */}
        <div style={{
          position: 'absolute',
          top: 12,
          right: 12,
          padding: '5px 10px',
          borderRadius: 9999,
          fontSize: '0.74rem',
          fontWeight: 700,
          display: 'flex',
          alignItems: 'center',
          gap: 5,
          backdropFilter: 'blur(8px)',
          background: isRead ? 'rgba(16, 185, 129, 0.92)' : 'rgba(245, 158, 11, 0.92)',
          color: '#fff',
          boxShadow: '0 2px 8px rgba(0,0,0,0.3)'
        }}>
          {isRead ? <Check size={12} strokeWidth={3} /> : <Clock size={12} strokeWidth={3} />}
          <span>{isRead ? 'Lido' : 'Na Fila'}</span>
        </div>
      </div>

      {/* Content */}
      <div style={{ padding: 16, display: 'flex', flexDirection: 'column', flex: 1 }}>
        <h4 style={{
          fontSize: '1rem',
          fontWeight: 700,
          color: 'var(--text-main)',
          marginBottom: 6,
          lineHeight: 1.35,
          display: '-webkit-box',
          WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
          minHeight: '2.7em'
        }}>
          {leitura.titulo}
        </h4>

        {/* Metadata info */}
        <div style={{ fontSize: '0.78rem', color: 'var(--text-muted)', display: 'flex', flexDirection: 'column', gap: 4, marginBottom: 14 }}>
          {leitura.dataPublicacao && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <Calendar size={13} style={{ color: 'var(--text-dim)' }} />
              <span>Publicado: <strong>{leitura.dataPublicacao}</strong></span>
            </div>
          )}

          {isRead && leitura.dataLeitura && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 6, color: 'var(--accent-emerald)' }}>
              <BookmarkCheck size={13} />
              <span>Concluído em: <strong>{formatDate(leitura.dataLeitura)}</strong></span>
            </div>
          )}
        </div>

        {/* Action Buttons */}
        <div style={{ marginTop: 'auto', display: 'flex', gap: 8, paddingTop: 10, borderTop: '1px solid var(--border-subtle)' }}>
          <button
            onClick={onToggleRead}
            title={isRead ? 'Marcar como Na Fila' : 'Marcar como Lido'}
            style={{
              flex: 1,
              padding: '8px 10px',
              borderRadius: 'var(--radius-sm)',
              border: 'none',
              background: isRead ? 'rgba(16, 185, 129, 0.16)' : 'var(--primary)',
              color: isRead ? 'var(--accent-emerald)' : '#fff',
              fontSize: '0.8rem',
              fontWeight: 700,
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 6,
              transition: 'background 0.2s ease'
            }}
          >
            {isRead ? (
              <>
                <Check size={14} />
                <span>Lido</span>
              </>
            ) : (
              <>
                <BookmarkCheck size={14} />
                <span>Marcar Lido</span>
              </>
            )}
          </button>

          <button
            onClick={onEdit}
            title="Editar data ou status"
            style={{
              padding: '8px 10px',
              borderRadius: 'var(--radius-sm)',
              border: '1px solid var(--border-subtle)',
              background: 'var(--bg-main)',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            <Edit3 size={15} />
          </button>

          <button
            onClick={onDelete}
            title="Excluir da leitura"
            style={{
              padding: '8px 10px',
              borderRadius: 'var(--radius-sm)',
              border: '1px solid rgba(239, 68, 68, 0.25)',
              background: 'rgba(239, 68, 68, 0.08)',
              color: '#ef4444',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            <Trash2 size={15} />
          </button>
        </div>
      </div>
    </div>
  );
};
