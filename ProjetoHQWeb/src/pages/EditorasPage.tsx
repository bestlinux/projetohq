import React, { useState, useEffect, useMemo } from 'react';
import { Building2, Plus, Search, Edit2, Trash2, RefreshCw, BookOpen, Check, X } from 'lucide-react';
import { Editora, HQ } from '../types';
import { apiService } from '../services/api';

export const EditorasPage: React.FC = () => {
  const [editoras, setEditoras] = useState<Editora[]>([]);
  const [hqs, setHqs] = useState<HQ[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);

  // Modal states
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [editingEditora, setEditingEditora] = useState<Editora | null>(null);
  const [formNome, setFormNome] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async (forceRefresh = false) => {
    if (forceRefresh) setIsRefreshing(true);
    else setIsLoading(true);

    try {
      const [edList, hqList] = await Promise.all([
        apiService.getEditoras(forceRefresh),
        apiService.getHQs(forceRefresh)
      ]);
      setEditoras(edList);
      setHqs(hqList);
    } catch (err) {
      console.error('Erro ao carregar editoras:', err);
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  };

  // Map counting comics per editora
  const hqCountPerEditora = useMemo(() => {
    const counts: Record<string, number> = {};
    for (const hq of hqs) {
      const edName = (hq.editora || '').trim().toLowerCase();
      if (edName) {
        counts[edName] = (counts[edName] || 0) + 1;
      }
    }
    return counts;
  }, [hqs]);

  // Filtered editoras
  const filteredEditoras = useMemo(() => {
    return editoras.filter(ed =>
      ed.nome.toLowerCase().includes(searchTerm.toLowerCase().trim())
    );
  }, [editoras, searchTerm]);

  // Open add modal
  const handleOpenAdd = () => {
    setFormNome('');
    setEditingEditora(null);
    setIsAddModalOpen(true);
  };

  // Open edit modal
  const handleOpenEdit = (ed: Editora) => {
    setFormNome(ed.nome);
    setEditingEditora(ed);
    setIsAddModalOpen(true);
  };

  // Save add/edit
  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    const cleanNome = formNome.trim();
    if (!cleanNome) return;

    setSaving(true);
    try {
      if (editingEditora) {
        const updated = await apiService.updateEditora(editingEditora.id, cleanNome);
        setEditoras(prev =>
          prev.map(item => (item.id === editingEditora.id ? updated : item))
            .sort((a, b) => a.nome.localeCompare(b.nome))
        );
      } else {
        const created = await apiService.addEditora(cleanNome);
        setEditoras(prev =>
          [...prev, created].sort((a, b) => a.nome.localeCompare(b.nome))
        );
      }
      setIsAddModalOpen(false);
      setFormNome('');
      setEditingEditora(null);
    } catch (err) {
      alert('Erro ao salvar editora. Verifique a conexão com a API.');
    } finally {
      setSaving(false);
    }
  };

  // Delete
  const handleDelete = async (ed: Editora) => {
    const comicsCount = hqCountPerEditora[ed.nome.toLowerCase()] || 0;
    const warning = comicsCount > 0
      ? `Atenção: Esta editora possui ${comicsCount} quadrinho(s) associado(s).\n\nDeseja realmente excluir "${ed.nome}"?`
      : `Deseja realmente excluir a editora "${ed.nome}"?`;

    if (!window.confirm(warning)) {
      return;
    }

    try {
      await apiService.removeEditora(ed.id);
      setEditoras(prev => prev.filter(item => item.id !== ed.id));
    } catch (err) {
      alert('Erro ao excluir editora. Tente novamente.');
    }
  };

  return (
    <div style={{ maxWidth: 1100, margin: '0 auto' }}>
      {/* Header Banner */}
      <div style={{
        marginBottom: 24,
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'flex-start',
        flexWrap: 'wrap',
        gap: 16
      }}>
        <div>
          <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--accent-amber)', letterSpacing: 1, textTransform: 'uppercase' }}>
            Gerenciamento de Selos
          </span>
          <h1 style={{ fontSize: '2rem', fontWeight: 800, marginTop: 2, letterSpacing: '-0.5px' }}>
            Editoras & Publicadoras
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            Cadastre, edite e organize as editoras da sua coleção diretamente no SQL Server.
          </p>
        </div>

        <div style={{ display: 'flex', gap: 10, alignItems: 'center' }}>
          <button
            type="button"
            className="btn-secondary"
            onClick={() => loadData(true)}
            disabled={isLoading || isRefreshing}
            title="Sincronizar editoras com o SQL Server"
            style={{ display: 'flex', alignItems: 'center', gap: 8, padding: '10px 16px' }}
          >
            <RefreshCw size={16} className={isRefreshing ? 'animate-spin' : ''} />
            <span>{isRefreshing ? 'Atualizando...' : 'Sincronizar'}</span>
          </button>

          <button
            type="button"
            className="btn-primary"
            onClick={handleOpenAdd}
            style={{ display: 'flex', alignItems: 'center', gap: 8 }}
          >
            <Plus size={18} />
            <span>Nova Editora</span>
          </button>
        </div>
      </div>

      {/* Stats bar & Search */}
      <div style={{
        display: 'flex',
        gap: 16,
        alignItems: 'center',
        justifyContent: 'space-between',
        flexWrap: 'wrap',
        marginBottom: 24
      }}>
        {/* Search */}
        <div style={{
          position: 'relative',
          flex: '1 1 300px',
          maxWidth: 480
        }}>
          <Search size={18} color="var(--text-dim)" style={{ position: 'absolute', left: 14, top: '50%', transform: 'translateY(-50%)' }} />
          <input
            type="text"
            placeholder="Buscar editora por nome..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            style={{
              width: '100%',
              padding: '11px 14px 11px 42px',
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 'var(--radius-sm)',
              color: 'var(--text-main)',
              fontSize: '0.9rem',
              outline: 'none'
            }}
          />
        </div>

        {/* Counter Badge */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: 10,
          background: 'var(--bg-card)',
          padding: '8px 16px',
          borderRadius: 'var(--radius-sm)',
          border: '1px solid var(--border-subtle)',
          fontSize: '0.85rem'
        }}>
          <Building2 size={16} color="var(--accent-amber)" />
          <span>
            <strong>{editoras.length}</strong> editoras cadastradas
          </span>
          {searchTerm && (
            <span style={{ color: 'var(--text-muted)' }}>
              ({filteredEditoras.length} encontradas)
            </span>
          )}
        </div>
      </div>

      {/* Grid of Editoras */}
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: '60px 20px', color: 'var(--text-muted)' }}>
          <RefreshCw size={28} className="animate-spin" style={{ margin: '0 auto 12px', color: 'var(--primary)' }} />
          <p>Carregando editoras do SQL Server...</p>
        </div>
      ) : filteredEditoras.length > 0 ? (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
          gap: 16
        }}>
          {filteredEditoras.map(ed => {
            const count = hqCountPerEditora[ed.nome.toLowerCase()] || 0;
            return (
              <div
                key={ed.id}
                style={{
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-md)',
                  padding: '18px 20px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: 12,
                  transition: 'all 0.2s ease',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.18)'
                }}
              >
                <div style={{ display: 'flex', alignItems: 'center', gap: 14, minWidth: 0 }}>
                  <div style={{
                    width: 44,
                    height: 44,
                    borderRadius: 'var(--radius-sm)',
                    background: 'linear-gradient(135deg, rgba(245, 158, 11, 0.15), rgba(99, 102, 241, 0.12))',
                    border: '1px solid rgba(245, 158, 11, 0.3)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: 'var(--accent-amber)',
                    flexShrink: 0
                  }}>
                    <Building2 size={22} />
                  </div>

                  <div style={{ minWidth: 0 }}>
                    <h3 style={{
                      fontSize: '1.02rem',
                      fontWeight: 700,
                      margin: 0,
                      color: 'var(--text-main)',
                      whiteSpace: 'nowrap',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis'
                    }} title={ed.nome}>
                      {ed.nome}
                    </h3>
                    <span style={{
                      fontSize: '0.78rem',
                      color: count > 0 ? 'var(--text-muted)' : 'var(--text-dim)',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 4,
                      marginTop: 3
                    }}>
                      <BookOpen size={12} />
                      {count > 0 ? `${count} quadrinho(s)` : 'Nenhum quadrinho'}
                    </span>
                  </div>
                </div>

                {/* Actions */}
                <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                  <button
                    type="button"
                    onClick={() => handleOpenEdit(ed)}
                    title="Editar editora"
                    style={{
                      background: 'rgba(255, 255, 255, 0.05)',
                      border: '1px solid var(--border-subtle)',
                      color: 'var(--text-main)',
                      width: 34,
                      height: 34,
                      borderRadius: 8,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      cursor: 'pointer',
                      transition: 'background 0.2s ease'
                    }}
                  >
                    <Edit2 size={15} />
                  </button>

                  <button
                    type="button"
                    onClick={() => handleDelete(ed)}
                    title="Excluir editora"
                    style={{
                      background: 'rgba(239, 68, 68, 0.08)',
                      border: '1px solid rgba(239, 68, 68, 0.25)',
                      color: 'var(--danger)',
                      width: 34,
                      height: 34,
                      borderRadius: 8,
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      cursor: 'pointer',
                      transition: 'background 0.2s ease'
                    }}
                  >
                    <Trash2 size={15} />
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <div style={{
          textAlign: 'center',
          padding: '60px 20px',
          background: 'var(--bg-card)',
          borderRadius: 'var(--radius-md)',
          border: '1px solid var(--border-subtle)'
        }}>
          <Building2 size={44} color="var(--text-dim)" style={{ margin: '0 auto 14px' }} />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 700 }}>Nenhuma editora encontrada</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4, maxWidth: 360, margin: '6px auto 16px' }}>
            {searchTerm ? 'Tente buscar com outro nome.' : 'Cadastre sua primeira editora para organizar seus quadrinhos.'}
          </p>
          <button
            type="button"
            className="btn-primary"
            onClick={handleOpenAdd}
          >
            <Plus size={18} />
            Cadastrar Editora
          </button>
        </div>
      )}

      {/* Add / Edit Modal */}
      {isAddModalOpen && (
        <div style={{
          position: 'fixed',
          inset: 0,
          background: 'rgba(0, 0, 0, 0.75)',
          backdropFilter: 'blur(8px)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          zIndex: 1000,
          padding: 16
        }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-md)',
            width: '100%',
            maxWidth: 440,
            padding: 24,
            boxShadow: '0 20px 50px rgba(0,0,0,0.5)',
            animation: 'fadeIn 0.2s ease-out'
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 18 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 36,
                  height: 36,
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(245, 158, 11, 0.15)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--accent-amber)'
                }}>
                  <Building2 size={20} />
                </div>
                <h3 style={{ fontSize: '1.2rem', fontWeight: 800 }}>
                  {editingEditora ? 'Editar Editora' : 'Nova Editora'}
                </h3>
              </div>

              <button
                type="button"
                onClick={() => setIsAddModalOpen(false)}
                style={{
                  background: 'transparent',
                  border: 'none',
                  color: 'var(--text-muted)',
                  cursor: 'pointer',
                  padding: 4
                }}
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSave}>
              <div style={{ marginBottom: 20 }}>
                <label style={{
                  display: 'block',
                  fontSize: '0.8rem',
                  fontWeight: 700,
                  marginBottom: 6,
                  color: 'var(--text-muted)',
                  textTransform: 'uppercase',
                  letterSpacing: 0.5
                }}>
                  Nome da Editora / Selo
                </label>
                <input
                  type="text"
                  autoFocus
                  required
                  placeholder="Ex: Panini Comics, Marvel, DC..."
                  value={formNome}
                  onChange={(e) => setFormNome(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '12px 14px',
                    background: 'var(--bg-surface)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.95rem',
                    outline: 'none'
                  }}
                />
              </div>

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => setIsAddModalOpen(false)}
                  disabled={saving}
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="btn-primary"
                  disabled={saving || !formNome.trim()}
                  style={{ display: 'flex', alignItems: 'center', gap: 6 }}
                >
                  <Check size={16} />
                  <span>{saving ? 'Salvando...' : editingEditora ? 'Salvar Alterações' : 'Cadastrar'}</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
