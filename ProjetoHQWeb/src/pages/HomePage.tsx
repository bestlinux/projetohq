import React, { useState, useEffect, useMemo } from 'react';
import { Search, Plus, BookOpen, CheckCircle2, Flame, Sparkles, RefreshCw, Database, X } from 'lucide-react';
import { HQ, Editora } from '../types';
import { apiService, getCapaUrl } from '../services/api';
import { HQCard } from '../components/HQCard';
import { HQModal } from '../components/HQModal';
import { AddHQModal } from '../components/AddHQModal';

export const HomePage: React.FC = () => {
  const [hqs, setHqs] = useState<HQ[]>([]);
  const [editoras, setEditoras] = useState<Editora[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedEditora, setSelectedEditora] = useState<string>('Todas');
  const [readFilter, setReadFilter] = useState<'all' | 'read' | 'unread'>('all');

  const [selectedHQ, setSelectedHQ] = useState<HQ | null>(null);
  const [expandedHQ, setExpandedHQ] = useState<HQ | null>(null);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [visibleCount, setVisibleCount] = useState(48);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async (forceRefresh = false) => {
    if (forceRefresh) setIsRefreshing(true);
    else setIsLoading(true);

    try {
      const [hqList, edList] = await Promise.all([
        apiService.getHQs(forceRefresh),
        apiService.getEditoras()
      ]);
      setHqs(hqList);
      setEditoras(edList);
    } catch (err) {
      console.error(err);
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  };

  const handleToggleRead = async (e: React.MouseEvent | null, hqId: string) => {
    if (e) e.stopPropagation();
    try {
      const updated = await apiService.toggleLido(hqId);
      setHqs(prev => prev.map(h => h.id === hqId ? updated : h));
      if (selectedHQ && selectedHQ.id === hqId) {
        setSelectedHQ(updated);
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleAddHQ = async (newHQData: Omit<HQ, 'id'>) => {
    const created = await apiService.addHQ(newHQData);
    setHqs(prev => [created, ...prev]);
  };

  const handleRemoveHQ = async (hqId: string) => {
    await apiService.removeHQ(hqId);
    setHqs(prev => prev.filter(h => h.id !== hqId));
    setSelectedHQ(null);
  };

  const handleMoveToWishlist = async (hq: HQ) => {
    await apiService.addDesejo({
      titulo: hq.titulo,
      editora: hq.editora,
      numeroEdicao: hq.numeroEdicao,
      thumbCapa: hq.thumbCapa,
      preco: hq.preco,
      personagens: hq.personagens,
      desenhosRoteirosArteFinalCores: hq.desenhosRoteirosArteFinalCores,
      lido: 0,
    });
    await handleRemoveHQ(hq.id);
    alert(`"${hq.titulo}" foi movida para sua Lista de Desejos!`);
  };

  // Filtered Comics
  const filteredHqs = useMemo(() => {
    return hqs.filter(hq => {
      const matchSearch = searchTerm.trim() === '' ||
        hq.titulo.toLowerCase().includes(searchTerm.toLowerCase()) ||
        hq.editora?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        hq.personagens?.toLowerCase().includes(searchTerm.toLowerCase());

      const matchEditora = selectedEditora === 'Todas' ||
        hq.editora?.toLowerCase() === selectedEditora.toLowerCase();

      const matchRead =
        readFilter === 'all' ? true :
        readFilter === 'read' ? hq.lido === 1 :
        hq.lido === 0;

      return matchSearch && matchEditora && matchRead;
    });
  }, [hqs, searchTerm, selectedEditora, readFilter]);

  // Statistics
  const totalLidos = useMemo(() => hqs.filter(h => h.lido === 1).length, [hqs]);
  const progressoLeitura = hqs.length > 0 ? Math.round((totalLidos / hqs.length) * 100) : 0;

  return (
    <div>
      {/* Header Banner & Stats */}
      <div style={{ marginBottom: 20, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16 }}>
        <div>
          <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--primary)', letterSpacing: 1, textTransform: 'uppercase' }}>
            Minha Coleção
          </span>
          <h1 style={{ fontSize: '2rem', fontWeight: 800, marginTop: 2, letterSpacing: '-0.5px' }}>
            Quadrinhos & HQs
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            Explore e organize seu acervo com busca instantânea.
          </p>
        </div>

        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <button
            type="button"
            className="btn-secondary"
            onClick={() => loadData(true)}
            disabled={isLoading || isRefreshing}
            title="Sincronizar quadrinhos com o SQL Server"
            style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '10px 16px' }}
          >
            <RefreshCw size={16} className={isRefreshing ? 'animate-spin' : ''} />
            <span>{isRefreshing ? 'Sincronizando...' : 'Sincronizar Banco'}</span>
          </button>

          <button
            type="button"
            className="btn-primary"
            onClick={() => setIsAddModalOpen(true)}
          >
            <Plus size={18} />
            <span>Cadastrar HQ</span>
          </button>
        </div>
      </div>

      {/* Database Connection Status Bar */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '12px 18px',
        borderRadius: 'var(--radius-sm)',
        background: hqs.length > 50 
          ? 'linear-gradient(90deg, rgba(16, 185, 129, 0.12), rgba(16, 185, 129, 0.04))' 
          : 'linear-gradient(90deg, rgba(245, 158, 11, 0.12), rgba(245, 158, 11, 0.04))',
        border: `1px solid ${hqs.length > 50 ? 'rgba(16, 185, 129, 0.28)' : 'rgba(245, 158, 11, 0.28)'}`,
        marginBottom: 20,
        fontSize: '0.86rem'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <Database size={18} color={hqs.length > 50 ? 'var(--accent-emerald)' : 'var(--accent-amber)'} />
          {isLoading ? (
            <span style={{ color: 'var(--text-main)', display: 'flex', alignItems: 'center', gap: 8 }}>
              <RefreshCw size={14} className="animate-spin" /> Conectando à API e carregando quadrinhos do SQL Server...
            </span>
          ) : hqs.length > 50 ? (
            <span style={{ color: 'var(--text-main)' }}>
              🟢 <strong>Base SQL Server Online (GROGU):</strong> {hqs.length} quadrinhos carregados em tempo real do banco de dados.
            </span>
          ) : (
            <span style={{ color: 'var(--text-muted)' }}>
              🟡 <strong>Modo Demonstração:</strong> Mostrando amostra local. Clique em "Sincronizar Banco" para carregar seus dados reais do SQL Server.
            </span>
          )}
        </div>

        {hqs.length > 50 && (
          <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
            Autenticação JWT ativa
          </span>
        )}
      </div>

      {/* Hero Stats */}
      <div className="hero-stats">
        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(99, 102, 241, 0.15)', color: 'var(--primary)' }}>
            <BookOpen size={22} />
          </div>
          <div>
            <div className="stat-value">{hqs.length}</div>
            <div className="stat-label">Total de HQs</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(16, 185, 129, 0.15)', color: 'var(--accent-emerald)' }}>
            <CheckCircle2 size={22} />
          </div>
          <div>
            <div className="stat-value">{totalLidos}</div>
            <div className="stat-label">Lidas ({progressoLeitura}%)</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(236, 72, 153, 0.15)', color: 'var(--accent-pink)' }}>
            <Flame size={22} />
          </div>
          <div>
            <div className="stat-value">{hqs.length - totalLidos}</div>
            <div className="stat-label">Na Fila</div>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon" style={{ background: 'rgba(245, 158, 11, 0.15)', color: 'var(--accent-amber)' }}>
            <Sparkles size={22} />
          </div>
          <div>
            <div className="stat-value">{editoras.length}</div>
            <div className="stat-label">Editoras</div>
          </div>
        </div>
      </div>

      {/* Search Input */}
      <div className="search-bar">
        <Search size={20} color="var(--text-dim)" />
        <input
          type="text"
          className="search-input"
          placeholder="Buscar por título, herói, roteirista ou editora..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        {searchTerm && (
          <button
            onClick={() => setSearchTerm('')}
            style={{ background: 'none', border: 'none', color: 'var(--text-dim)', cursor: 'pointer', padding: 4 }}
          >
            ✕
          </button>
        )}
      </div>

      {/* Filter Chips by Publisher */}
      <div className="filter-chips">
        <button
          className={`chip ${selectedEditora === 'Todas' ? 'active' : ''}`}
          onClick={() => setSelectedEditora('Todas')}
        >
          Todas
        </button>
        {editoras.map(ed => (
          <button
            key={ed.id}
            className={`chip ${selectedEditora.toLowerCase() === ed.nome.toLowerCase() ? 'active' : ''}`}
            onClick={() => setSelectedEditora(ed.nome)}
          >
            {ed.nome}
          </button>
        ))}
      </div>

      {/* Secondary filter: Read / Unread toggle */}
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
        <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)', fontWeight: 600 }}>
          Exibindo {filteredHqs.length} {filteredHqs.length === 1 ? 'edição' : 'edições'}
        </span>

        <div style={{ display: 'flex', gap: 6 }}>
          <button
            onClick={() => setReadFilter('all')}
            className={`chip ${readFilter === 'all' ? 'active' : ''}`}
            style={{ padding: '4px 12px', fontSize: '0.75rem' }}
          >
            Todas
          </button>
          <button
            onClick={() => setReadFilter('read')}
            className={`chip ${readFilter === 'read' ? 'active' : ''}`}
            style={{ padding: '4px 12px', fontSize: '0.75rem' }}
          >
            Lidas
          </button>
          <button
            onClick={() => setReadFilter('unread')}
            className={`chip ${readFilter === 'unread' ? 'active' : ''}`}
            style={{ padding: '4px 12px', fontSize: '0.75rem' }}
          >
            Não Lidas
          </button>
        </div>
      </div>

      {/* Comic Grid */}
      {filteredHqs.length > 0 ? (
        <>
          <div className="hq-grid">
            {filteredHqs.slice(0, visibleCount).map((hq) => (
              <HQCard
                key={hq.id}
                hq={hq}
                onSelect={(selected) => setSelectedHQ(selected)}
                onToggleRead={(e, id) => handleToggleRead(e, id)}
                onExpand={(expanded) => setExpandedHQ(expanded)}
              />
            ))}
          </div>

          {visibleCount < filteredHqs.length && (
            <div style={{ textAlign: 'center', marginTop: 28, marginBottom: 20 }}>
              <button
                type="button"
                className="btn-secondary"
                onClick={() => setVisibleCount(prev => prev + 48)}
                style={{ padding: '12px 28px', fontSize: '0.9rem', fontWeight: 700 }}
              >
                Carregar mais quadrinhos ({filteredHqs.length - visibleCount} restantes)
              </button>
            </div>
          )}
        </>
      ) : (
        <div style={{
          textAlign: 'center',
          padding: '60px 20px',
          background: 'var(--bg-card)',
          borderRadius: 'var(--radius-md)',
          border: '1px solid var(--border-subtle)'
        }}>
          <BookOpen size={48} color="var(--text-dim)" style={{ margin: '0 auto 16px' }} />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 700 }}>Nenhum quadrinho encontrado</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4, maxWidth: 400, margin: '6px auto 16px' }}>
            Tente mudar o termo de busca ou adicione um novo quadrinho à sua coleção.
          </p>
          <button
            type="button"
            className="btn-primary"
            onClick={() => setIsAddModalOpen(true)}
          >
            <Plus size={18} />
            Cadastrar Novo Quadrinho
          </button>
        </div>
      )}

      {/* HQ Details Modal */}
      <HQModal
        hq={selectedHQ}
        onClose={() => setSelectedHQ(null)}
        onToggleRead={(id) => handleToggleRead(null, id)}
        onRemove={(id) => handleRemoveHQ(id)}
        onMoveToWishlist={(hq) => handleMoveToWishlist(hq)}
      />

      {/* Add HQ Modal */}
      <AddHQModal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        onAdd={handleAddHQ}
        editoras={editoras}
      />

      {/* Lightbox / Modal Expandir Capa da HQ */}
      {expandedHQ && (
        <div 
          className="modal-backdrop" 
          onClick={() => setExpandedHQ(null)} 
          style={{ 
            position: 'fixed',
            inset: 0,
            zIndex: 1300,
            background: 'rgba(0, 0, 0, 0.88)',
            backdropFilter: 'blur(10px)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            padding: 20
          }}
        >
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ 
              maxWidth: 820, 
              width: '100%', 
              padding: 0, 
              overflow: 'hidden', 
              background: '#0d1322',
              borderRadius: 'var(--radius-md)',
              border: '1px solid rgba(255, 255, 255, 0.12)',
              boxShadow: '0 25px 60px rgba(0, 0, 0, 0.7)',
              display: 'flex',
              flexDirection: 'column'
            }}
          >
            {/* Header */}
            <div style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              padding: '16px 20px',
              borderBottom: '1px solid var(--border-subtle)',
              background: 'rgba(15, 23, 42, 0.85)'
            }}>
              <div>
                <h3 style={{ fontSize: '1.2rem', fontWeight: 800, margin: 0, color: 'var(--text-main)' }}>
                  {expandedHQ.titulo} (Edição #{expandedHQ.numeroEdicao > 0 ? expandedHQ.numeroEdicao : '1'})
                </h3>
                <div style={{ fontSize: '0.85rem', color: 'var(--accent-pink)', marginTop: 2, fontWeight: 600 }}>
                  {expandedHQ.editora || 'Independente'} {expandedHQ.dataPublicacao ? `• ${expandedHQ.dataPublicacao}` : ''}
                </div>
              </div>
              <button
                type="button"
                onClick={() => setExpandedHQ(null)}
                title="Fechar"
                style={{ 
                  background: 'rgba(255, 255, 255, 0.1)', 
                  border: 'none', 
                  borderRadius: '50%',
                  width: 36,
                  height: 36,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--text-main)', 
                  cursor: 'pointer' 
                }}
              >
                <X size={20} />
              </button>
            </div>

            {/* Image Preview Body */}
            <div style={{
              padding: 20,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              maxHeight: '75vh',
              overflow: 'auto',
              background: '#070a13'
            }}>
              <img
                src={getCapaUrl(expandedHQ.capa) || expandedHQ.thumbCapa}
                alt={expandedHQ.titulo}
                style={{ 
                  maxWidth: '100%', 
                  maxHeight: '70vh', 
                  objectFit: 'contain', 
                  borderRadius: 'var(--radius-sm)',
                  boxShadow: '0 10px 30px rgba(0, 0, 0, 0.5)'
                }}
              />
            </div>

            {/* Footer */}
            <div style={{ 
              padding: '12px 20px', 
              fontSize: '0.8rem', 
              color: 'var(--text-muted)', 
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              background: 'rgba(15, 23, 42, 0.85)',
              borderTop: '1px solid var(--border-subtle)'
            }}>
              <span>Arquivo: <strong style={{ color: 'var(--text-main)' }}>{expandedHQ.capa || 'Capa Remota'}</strong></span>
              <button
                type="button"
                onClick={() => setExpandedHQ(null)}
                className="btn-secondary"
                style={{ padding: '6px 16px', fontSize: '0.82rem', borderRadius: 'var(--radius-sm)' }}
              >
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
