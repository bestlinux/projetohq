import React, { useState, useEffect, useMemo } from 'react';
import { 
  Quote, 
  Plus, 
  Search, 
  Edit3, 
  Trash2, 
  RefreshCw, 
  BookOpen, 
  User, 
  Maximize2, 
  X, 
  AlertCircle, 
  Upload, 
  Image as ImageIcon,
  Sparkles
} from 'lucide-react';
import { Frase, HQ } from '../types';
import { apiService, getFraseImagemUrl } from '../services/api';

export const FrasesPage: React.FC = () => {
  const [frases, setFrases] = useState<Frase[]>([]);
  const [hqs, setHqs] = useState<HQ[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');

  // Modals
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [editingFrase, setEditingFrase] = useState<Frase | null>(null);
  const [deletingFrase, setDeletingFrase] = useState<Frase | null>(null);
  const [previewFrase, setPreviewFrase] = useState<Frase | null>(null);

  // Form states
  const [formNomeHQ, setFormNomeHQ] = useState('');
  const [formAutor, setFormAutor] = useState('');
  const [formArquivo, setFormArquivo] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadMode, setUploadMode] = useState<'upload' | 'filename'>('upload');
  const [saving, setSaving] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async (forceRefresh = false) => {
    if (forceRefresh) setIsRefreshing(true);
    else setIsLoading(true);

    try {
      const [frasesList, hqList] = await Promise.all([
        apiService.getFrases(undefined, forceRefresh),
        apiService.getHQs(false).catch(() => [])
      ]);
      setFrases(frasesList);
      setHqs(hqList);
    } catch (err) {
      console.error('Erro ao carregar frases:', err);
    } finally {
      setIsLoading(false);
      setIsRefreshing(false);
    }
  };

  // Metrics
  const totalCount = frases.length;
  const uniqueHQs = useMemo(() => {
    const set = new Set(frases.map(f => f.nomeHQ.trim().toLowerCase()).filter(Boolean));
    return set.size;
  }, [frases]);

  const uniqueAutores = useMemo(() => {
    const set = new Set(frases.map(f => f.autor.trim().toLowerCase()).filter(Boolean));
    return set.size;
  }, [frases]);

  // Filtered
  const filteredFrases = useMemo(() => {
    return frases.filter(item => {
      const term = searchTerm.toLowerCase().trim();
      if (!term) return true;
      return (
        item.nomeHQ.toLowerCase().includes(term) ||
        item.autor.toLowerCase().includes(term) ||
        item.arquivo.toLowerCase().includes(term)
      );
    });
  }, [frases, searchTerm]);

  // Open Add
  const handleOpenAdd = () => {
    setFormNomeHQ('');
    setFormAutor('');
    setFormArquivo('');
    setSelectedFile(null);
    setUploadMode('upload');
    setErrorMessage('');
    setIsAddModalOpen(true);
  };

  // Open Edit
  const handleOpenEdit = (frase: Frase) => {
    setEditingFrase(frase);
    setFormNomeHQ(frase.nomeHQ);
    setFormAutor(frase.autor);
    setFormArquivo(frase.arquivo);
    setErrorMessage('');
  };

  // Save Add
  const handleSaveAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formNomeHQ.trim()) {
      setErrorMessage('O nome da HQ é obrigatório.');
      return;
    }
    if (!formAutor.trim()) {
      setErrorMessage('O nome do autor/roteirista é obrigatório.');
      return;
    }

    if (uploadMode === 'upload' && !selectedFile) {
      setErrorMessage('Selecione uma imagem para enviar.');
      return;
    }
    if (uploadMode === 'filename' && !formArquivo.trim()) {
      setErrorMessage('Informe o nome do arquivo da imagem.');
      return;
    }

    setSaving(true);
    setErrorMessage('');

    try {
      let created: Frase;
      if (uploadMode === 'upload' && selectedFile) {
        created = await apiService.uploadFraseImage(formNomeHQ.trim(), formAutor.trim(), selectedFile);
      } else {
        created = await apiService.addFrase({
          nomeHq: formNomeHQ.trim(),
          autor: formAutor.trim(),
          arquivo: formArquivo.trim()
        });
      }

      setFrases(prev => [created, ...prev.filter(f => f.id !== created.id)]);
      setIsAddModalOpen(false);
    } catch (err: any) {
      setErrorMessage(err.message || 'Erro ao adicionar frase.');
    } finally {
      setSaving(false);
    }
  };

  // Save Edit
  const handleSaveEdit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingFrase) return;
    if (!formNomeHQ.trim()) {
      setErrorMessage('O nome da HQ é obrigatório.');
      return;
    }

    setSaving(true);
    try {
      await apiService.updateFrase(editingFrase.id, {
        nomeHq: formNomeHQ.trim(),
        autor: formAutor.trim(),
        arquivo: formArquivo.trim()
      });

      setFrases(prev => prev.map(f => f.id === editingFrase.id ? {
        ...f,
        nomeHQ: formNomeHQ.trim(),
        autor: formAutor.trim(),
        arquivo: formArquivo.trim() || f.arquivo
      } : f));
      setEditingFrase(null);
    } catch (err: any) {
      setErrorMessage(err.message || 'Erro ao atualizar frase.');
    } finally {
      setSaving(false);
    }
  };

  // Confirm Delete
  const handleConfirmDelete = async () => {
    if (!deletingFrase) return;
    setSaving(true);
    try {
      await apiService.removeFrase(deletingFrase.id);
      setFrases(prev => prev.filter(f => f.id !== deletingFrase.id));
      setDeletingFrase(null);
    } catch (err) {
      console.error('Erro ao excluir frase:', err);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div style={{ maxWidth: 1400, margin: '0 auto', paddingBottom: 60 }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16, marginBottom: 24 }}>
        <div>
          <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--primary)', letterSpacing: 1, textTransform: 'uppercase' }}>
            Endpoint api/Frases
          </span>
          <h1 style={{ fontSize: '2.1rem', fontWeight: 800, marginTop: 4, letterSpacing: '-0.5px' }}>
            Frases & Painéis Marcantes
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.92rem', marginTop: 4 }}>
            Galeria de citações, diálogos inesquecíveis e quadros memoráveis dos quadrinhos salvos em repositório.
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
            <span>Nova Frase</span>
          </button>
        </div>
      </div>

      {/* Metrics Banner */}
      <div style={{
        background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.12), rgba(15, 23, 42, 0.85))',
        border: '1px solid rgba(99, 102, 241, 0.3)',
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
              background: 'rgba(99, 102, 241, 0.22)',
              border: '1px solid rgba(99, 102, 241, 0.4)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)'
            }}>
              <Quote size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>Total de Frases</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: 'var(--text-main)', lineHeight: 1.2 }}>{totalCount}</div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <div style={{
              width: 48,
              height: 48,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(236, 72, 153, 0.18)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--accent-pink)'
            }}>
              <BookOpen size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>HQs Citadas</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: 'var(--accent-pink)', lineHeight: 1.2 }}>{uniqueHQs}</div>
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
              color: 'var(--accent-amber)'
            }}>
              <User size={24} />
            </div>
            <div>
              <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', textTransform: 'uppercase', fontWeight: 600 }}>Autores / Criadores</div>
              <div style={{ fontSize: '1.7rem', fontWeight: 800, color: 'var(--accent-amber)', lineHeight: 1.2 }}>{uniqueAutores}</div>
            </div>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 10, background: 'rgba(0,0,0,0.3)', padding: '12px 16px', borderRadius: 'var(--radius-sm)', border: '1px solid var(--border-subtle)' }}>
            <Sparkles size={20} color="var(--primary)" />
            <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
              Imagens vinculadas da pasta <br />
              <strong style={{ color: 'var(--text-main)' }}>projetohqrepositorio\Frases</strong>
            </div>
          </div>
        </div>
      </div>

      {/* Search Input */}
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
        <div style={{ fontSize: '0.9rem', fontWeight: 700, color: 'var(--text-main)' }}>
          Mostrando {filteredFrases.length} de {totalCount} frases
        </div>

        <div style={{ position: 'relative', width: 340, maxWidth: '100%' }}>
          <Search size={17} style={{ position: 'absolute', left: 12, top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input
            type="text"
            placeholder="Buscar por HQ ou Autor..."
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

      {/* Grid of Frases */}
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: '80px 20px', color: 'var(--text-muted)' }}>
          <RefreshCw size={36} className="animate-spin" style={{ margin: '0 auto 12px', color: 'var(--primary)' }} />
          <div>Carregando frases do banco SQL Server...</div>
        </div>
      ) : filteredFrases.length > 0 ? (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
          gap: 22
        }}>
          {filteredFrases.map(item => (
            <FraseCard
              key={item.id}
              frase={item}
              onPreview={() => setPreviewFrase(item)}
              onEdit={() => handleOpenEdit(item)}
              onDelete={() => setDeletingFrase(item)}
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
          <Quote size={44} color="var(--text-dim)" style={{ margin: '0 auto 14px' }} />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 700 }}>Nenhuma frase encontrada</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            {searchTerm ? 'Nenhum resultado corresponde à sua pesquisa.' : 'Adicione sua primeira frase ou painel memorável.'}
          </p>
        </div>
      )}

      {/* Modal Preview Imagem Completa */}
      {previewFrase && (
        <div 
          className="modal-backdrop" 
          onClick={() => setPreviewFrase(null)} 
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
              maxWidth: 900, 
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
                  {previewFrase.nomeHQ}
                </h3>
                <div style={{ fontSize: '0.85rem', color: 'var(--accent-pink)', marginTop: 2, fontWeight: 600 }}>
                  Autor: {previewFrase.autor}
                </div>
              </div>
              <button
                type="button"
                onClick={() => setPreviewFrase(null)}
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
                src={getFraseImagemUrl(previewFrase.arquivo)}
                alt={previewFrase.nomeHQ}
                style={{ 
                  maxWidth: '100%', 
                  maxHeight: '70vh', 
                  objectFit: 'contain', 
                  borderRadius: 'var(--radius-sm)',
                  boxShadow: '0 10px 30px rgba(0, 0, 0, 0.5)'
                }}
              />
            </div>

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
              <span>Arquivo: <strong style={{ color: 'var(--text-main)' }}>{previewFrase.arquivo}</strong></span>
              <button
                type="button"
                onClick={() => setPreviewFrase(null)}
                className="btn-secondary"
                style={{ padding: '6px 16px', fontSize: '0.82rem', borderRadius: 'var(--radius-sm)' }}
              >
                Fechar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal Adicionar Frase */}
      {isAddModalOpen && (
        <div className="modal-backdrop" onClick={() => setIsAddModalOpen(false)}>
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 540, width: '92%' }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                <div style={{
                  width: 38,
                  height: 38,
                  borderRadius: 'var(--radius-sm)',
                  background: 'rgba(99, 102, 241, 0.15)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--primary)'
                }}>
                  <Plus size={20} />
                </div>
                <h3 style={{ fontSize: '1.25rem', fontWeight: 700, margin: 0 }}>Nova Frase / Painel</h3>
              </div>
              <button
                onClick={() => setIsAddModalOpen(false)}
                style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}
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
              {/* Seleção rápida da Coleção (Opcional) */}
              {hqs.length > 0 && (
                <div style={{ marginBottom: 16 }}>
                  <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                    Importar da Coleção (Opcional):
                  </label>
                  <select
                    onChange={(e) => {
                      const hq = hqs.find(h => h.id === e.target.value);
                      if (hq) {
                        setFormNomeHQ(hq.titulo);
                        if (hq.desenhosRoteirosArteFinalCores) {
                          setFormAutor(hq.desenhosRoteirosArteFinalCores);
                        }
                      }
                    }}
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

              {/* NomeHQ */}
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Nome da HQ / Título *
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ex: Sandman, Watchmen, Batman"
                  value={formNomeHQ}
                  onChange={(e) => setFormNomeHQ(e.target.value)}
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

              {/* Autor */}
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Autor / Roteirista *
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ex: Neil Gaiman, Alan Moore, Frank Miller"
                  value={formAutor}
                  onChange={(e) => setFormAutor(e.target.value)}
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

              {/* Upload Mode Selector */}
              <div style={{ marginBottom: 12 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Forma da Imagem
                </label>
                <div style={{ display: 'flex', gap: 10 }}>
                  <button
                    type="button"
                    onClick={() => setUploadMode('upload')}
                    style={{
                      flex: 1,
                      padding: '8px 12px',
                      borderRadius: 'var(--radius-sm)',
                      border: uploadMode === 'upload' ? '1px solid var(--primary)' : '1px solid var(--border-subtle)',
                      background: uploadMode === 'upload' ? 'rgba(99, 102, 241, 0.15)' : 'var(--bg-main)',
                      color: uploadMode === 'upload' ? 'var(--primary)' : 'var(--text-muted)',
                      fontWeight: 600,
                      cursor: 'pointer',
                      fontSize: '0.85rem'
                    }}
                  >
                    Fazer Upload de Imagem
                  </button>
                  <button
                    type="button"
                    onClick={() => setUploadMode('filename')}
                    style={{
                      flex: 1,
                      padding: '8px 12px',
                      borderRadius: 'var(--radius-sm)',
                      border: uploadMode === 'filename' ? '1px solid var(--primary)' : '1px solid var(--border-subtle)',
                      background: uploadMode === 'filename' ? 'rgba(99, 102, 241, 0.15)' : 'var(--bg-main)',
                      color: uploadMode === 'filename' ? 'var(--primary)' : 'var(--text-muted)',
                      fontWeight: 600,
                      cursor: 'pointer',
                      fontSize: '0.85rem'
                    }}
                  >
                    Nome de Arquivo Existente
                  </button>
                </div>
              </div>

              {uploadMode === 'upload' ? (
                <div style={{ marginBottom: 24 }}>
                  <div style={{
                    border: '2px dashed var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    padding: '20px 14px',
                    textAlign: 'center',
                    background: 'var(--bg-main)',
                    cursor: 'pointer'
                  }}>
                    <input
                      type="file"
                      accept="image/*"
                      id="frase-file-upload"
                      style={{ display: 'none' }}
                      onChange={(e) => {
                        if (e.target.files && e.target.files[0]) {
                          setSelectedFile(e.target.files[0]);
                        }
                      }}
                    />
                    <label htmlFor="frase-file-upload" style={{ cursor: 'pointer', display: 'block' }}>
                      <Upload size={28} color="var(--primary)" style={{ margin: '0 auto 8px' }} />
                      <div style={{ fontSize: '0.88rem', fontWeight: 600 }}>
                        {selectedFile ? selectedFile.name : 'Clique para selecionar uma imagem'}
                      </div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-dim)', marginTop: 4 }}>
                        A imagem será salva na pasta projetohqrepositorio\Frases
                      </div>
                    </label>
                  </div>
                </div>
              ) : (
                <div style={{ marginBottom: 24 }}>
                  <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                    Nome do Arquivo na pasta Frases *
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="Ex: 74330b692b244ad52f28e125b04e755d.jpg"
                    value={formArquivo}
                    onChange={(e) => setFormArquivo(e.target.value)}
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
                  {saving ? 'Salvando...' : 'Salvar Frase'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal Editar Frase */}
      {editingFrase && (
        <div className="modal-backdrop" onClick={() => setEditingFrase(null)}>
          <div
            className="modal-content"
            onClick={(e) => e.stopPropagation()}
            style={{ maxWidth: 500, width: '92%' }}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 18 }}>
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
                  <Edit3 size={18} />
                </div>
                <h3 style={{ fontSize: '1.2rem', fontWeight: 700, margin: 0 }}>Editar Frase</h3>
              </div>
              <button
                onClick={() => setEditingFrase(null)}
                style={{ background: 'transparent', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}
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
                fontSize: '0.85rem'
              }}>
                {errorMessage}
              </div>
            )}

            <form onSubmit={handleSaveEdit}>
              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Nome da HQ *
                </label>
                <input
                  type="text"
                  required
                  value={formNomeHQ}
                  onChange={(e) => setFormNomeHQ(e.target.value)}
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

              <div style={{ marginBottom: 16 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Autor *
                </label>
                <input
                  type="text"
                  required
                  value={formAutor}
                  onChange={(e) => setFormAutor(e.target.value)}
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

              <div style={{ marginBottom: 22 }}>
                <label style={{ display: 'block', fontSize: '0.82rem', fontWeight: 600, color: 'var(--text-muted)', marginBottom: 6 }}>
                  Arquivo da Imagem (em projetohqrepositorio\Frases)
                </label>
                <input
                  type="text"
                  value={formArquivo}
                  onChange={(e) => setFormArquivo(e.target.value)}
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
                  onClick={() => setEditingFrase(null)}
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
      {deletingFrase && (
        <div className="modal-backdrop" onClick={() => setDeletingFrase(null)}>
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
              Excluir Frase?
            </h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', marginBottom: 20 }}>
              Tem certeza que deseja excluir o registro de <strong>"{deletingFrase.nomeHQ}"</strong>? O item será removido do banco de dados e seu arquivo de imagem correspondente.
            </p>

            <div style={{ display: 'flex', justifyContent: 'center', gap: 12 }}>
              <button
                type="button"
                onClick={() => setDeletingFrase(null)}
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

// Card Component for individual Frase
interface FraseCardProps {
  frase: Frase;
  onPreview: () => void;
  onEdit: () => void;
  onDelete: () => void;
}

const FraseCard: React.FC<FraseCardProps> = ({
  frase,
  onPreview,
  onEdit,
  onDelete
}) => {
  const [imgError, setImgError] = useState(false);
  const imageUrl = getFraseImagemUrl(frase.arquivo);
  const showImage = imageUrl && !imgError;

  return (
    <div style={{
      background: 'var(--bg-card)',
      borderRadius: 'var(--radius-md)',
      border: '1px solid var(--border-subtle)',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column',
      boxShadow: '0 6px 20px rgba(0,0,0,0.22)',
      transition: 'transform 0.2s ease, border-color 0.2s ease',
      position: 'relative'
    }}>
      {/* Panel / Comic Image Area */}
      <div 
        onClick={onPreview}
        title="Clique para visualizar em tela cheia"
        style={{
          position: 'relative',
          height: 240,
          background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.95), rgba(15, 23, 42, 0.98))',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          overflow: 'hidden',
          cursor: 'pointer'
        }}
      >
        {showImage ? (
          <img
            src={imageUrl}
            alt={frase.nomeHQ}
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
              width: 48,
              height: 48,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(99, 102, 241, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)'
            }}>
              <ImageIcon size={24} />
            </div>
            <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', fontWeight: 600 }}>
              Imagem não disponível
            </span>
          </div>
        )}

        {/* Zoom Overlay Button */}
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onPreview();
          }}
          title="Expandir imagem"
          style={{
            position: 'absolute',
            top: 10,
            right: 10,
            width: 36,
            height: 36,
            borderRadius: 8,
            background: 'rgba(0, 0, 0, 0.72)',
            backdropFilter: 'blur(6px)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            border: '1px solid rgba(255, 255, 255, 0.25)',
            cursor: 'pointer',
            boxShadow: '0 4px 10px rgba(0,0,0,0.5)',
            transition: 'all 0.2s ease'
          }}
        >
          <Maximize2 size={16} />
        </button>
      </div>

      {/* Info & Content */}
      <div style={{ padding: '16px 18px', display: 'flex', flexDirection: 'column', flex: 1 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 6 }}>
          <Quote size={15} color="var(--primary)" />
          <h4 style={{
            fontSize: '1.05rem',
            fontWeight: 800,
            color: 'var(--text-main)',
            margin: 0,
            lineHeight: 1.3
          }}>
            {frase.nomeHQ}
          </h4>
        </div>

        <div style={{
          display: 'inline-flex',
          alignItems: 'center',
          gap: 6,
          fontSize: '0.82rem',
          color: 'var(--accent-pink)',
          fontWeight: 600,
          marginBottom: 12
        }}>
          <User size={13} />
          <span>{frase.autor}</span>
        </div>

        {/* Footer Actions */}
        <div style={{
          marginTop: 'auto',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          paddingTop: 10,
          borderTop: '1px solid var(--border-subtle)'
        }}>
          <span style={{ fontSize: '0.72rem', color: 'var(--text-dim)', maxWidth: '60%', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {frase.arquivo}
          </span>

          <div style={{ display: 'flex', gap: 6 }}>
            <button
              type="button"
              onClick={onPreview}
              title="Expandir imagem"
              style={{
                padding: '6px 10px',
                borderRadius: 'var(--radius-sm)',
                border: '1px solid var(--border-subtle)',
                background: 'var(--bg-main)',
                color: 'var(--primary)',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
              }}
            >
              <Maximize2 size={14} />
            </button>

            <button
              type="button"
              onClick={onEdit}
              title="Editar frase"
              style={{
                padding: '6px 10px',
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
              <Edit3 size={14} />
            </button>

            <button
              onClick={onDelete}
              title="Excluir frase"
              style={{
                padding: '6px 10px',
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
              <Trash2 size={14} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
