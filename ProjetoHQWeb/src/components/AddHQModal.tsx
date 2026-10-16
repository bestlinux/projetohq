import React, { useState, useRef } from 'react';
import { X, Plus, Globe, Upload, Search, Loader2, BookOpen, Check } from 'lucide-react';
import { HQ, Editora } from '../types';
import { apiService } from '../services/api';

interface AddHQModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAdd: (hq: Omit<HQ, 'id'>, coverFile?: File) => Promise<void> | void;
  editoras: Editora[];
  initialTab?: 'manual' | 'web';
  initialSearch?: {
    titulo?: string;
    editora?: string;
    edicao?: number;
  };
  onSuccessImport?: (importedItem?: HQ) => void;
}

export const AddHQModal: React.FC<AddHQModalProps> = ({ 
  isOpen, 
  onClose, 
  onAdd, 
  editoras,
  initialTab = 'manual',
  initialSearch,
  onSuccessImport
}) => {
  const [activeTab, setActiveTab] = useState<'manual' | 'web'>(initialTab);

  // Manual Form States
  const [titulo, setTitulo] = useState('');
  const [editora, setEditora] = useState(editoras[0]?.nome || 'Panini Comics');
  const [numeroEdicao, setNumeroEdicao] = useState('1');
  const [preco, setPreco] = useState('');
  const [numeroPaginas, setNumeroPaginas] = useState('');
  const [dataPublicacao, setDataPublicacao] = useState('');
  const [licenciador, setLicenciador] = useState('');
  const [personagens, setPersonagens] = useState('');
  const [desenhos, setDesenhos] = useState('');
  const [selectedCoverFile, setSelectedCoverFile] = useState<File | null>(null);
  const [coverPreview, setCoverPreview] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Web Search States
  const [webQueryTitulo, setWebQueryTitulo] = useState('');
  const [webQueryEditora, setWebQueryEditora] = useState('');
  const [webQueryEdicao, setWebQueryEdicao] = useState('');
  const [isSearchingWeb, setIsSearchingWeb] = useState(false);
  const [webResults, setWebResults] = useState<HQ[]>([]);
  const [hasSearchedWeb, setHasSearchedWeb] = useState(false);
  const [importingItemId, setImportingItemId] = useState<string | null>(null);
  const lastSearchedKeyRef = useRef<string>('');

  // Auto pre-fill and auto-search if initialSearch is provided (apenas uma única vez por item)
  React.useEffect(() => {
    if (isOpen) {
      if (initialTab) {
        setActiveTab(initialTab);
      }
      if (initialSearch) {
        const rawTitle = (initialSearch.titulo || '').trim();
        const cleaned = rawTitle
          .replace(/:\s*.*$/, '')
          .replace(/\s+-\s+.*$/, '')
          .replace(/\s+vol(ume)?\.?\s*\d+.*$/i, '')
          .trim();
        const t = cleaned.length >= 2 ? cleaned : rawTitle;
        const ed = (initialSearch.editora || '').trim();
        const num = initialSearch.edicao ? String(initialSearch.edicao) : '';
        setWebQueryTitulo(t);
        setWebQueryEditora(ed);
        setWebQueryEdicao(num);

        const currentKey = `${t}|${ed}|${num}`;
        if (t && lastSearchedKeyRef.current !== currentKey) {
          lastSearchedKeyRef.current = currentKey;
          setIsSearchingWeb(true);
          setHasSearchedWeb(true);
          apiService.buscaWeb({
            titulo: t,
            editora: ed || undefined,
            numeroEdicao: initialSearch.edicao || undefined
          }).then(results => {
            setWebResults(results);
          }).catch(err => {
            console.warn('Erro ao buscar web:', err);
          }).finally(() => {
            setIsSearchingWeb(false);
          });
        }
      }
    } else {
      lastSearchedKeyRef.current = '';
    }
  }, [isOpen, initialTab, initialSearch?.titulo, initialSearch?.editora, initialSearch?.edicao]);

  if (!isOpen) return null;

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setSelectedCoverFile(file);
      setCoverPreview(URL.createObjectURL(file));
    }
  };

  const handleReset = () => {
    setTitulo('');
    setPreco('');
    setNumeroEdicao('1');
    setNumeroPaginas('');
    setDataPublicacao('');
    setLicenciador('');
    setPersonagens('');
    setDesenhos('');
    setSelectedCoverFile(null);
    setCoverPreview(null);
    setWebResults([]);
    setHasSearchedWeb(false);
    setImportingItemId(null);
    onClose();
  };

  const handleSubmitManual = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) return;

    setIsSubmitting(true);
    try {
      await onAdd({
        titulo: titulo.trim(),
        editora: editora || 'Independente',
        numeroEdicao: parseInt(numeroEdicao, 10) || 1,
        preco: preco.trim() ? (preco.startsWith('R$') ? preco : `R$ ${preco}`) : undefined,
        numeroPaginas: parseInt(numeroPaginas, 10) || undefined,
        dataPublicacao: dataPublicacao.trim() || undefined,
        licenciador: licenciador.trim() || undefined,
        personagens: personagens.trim() || undefined,
        desenhosRoteirosArteFinalCores: desenhos.trim() || undefined,
        lido: 0,
      }, selectedCoverFile || undefined);

      handleReset();
    } catch (err) {
      alert('Erro ao cadastrar quadrinho.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSearchWeb = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!webQueryTitulo.trim()) return;

    setIsSearchingWeb(true);
    setHasSearchedWeb(true);
    try {
      const results = await apiService.buscaWeb({
        titulo: webQueryTitulo.trim(),
        editora: webQueryEditora.trim() || undefined,
        numeroEdicao: parseInt(webQueryEdicao, 10) || undefined
      });
      setWebResults(results);
    } catch (err) {
      alert('Erro ao buscar no Guia dos Quadrinhos.');
    } finally {
      setIsSearchingWeb(false);
    }
  };

  const handleImportWebItem = async (item: HQ) => {
    const currentItemId = item.id || `${item.titulo}-${item.numeroEdicao}-${item.editora}`;
    setImportingItemId(currentItemId);
    setIsSubmitting(true);
    try {
      await onAdd({
        titulo: item.titulo,
        editora: item.editora,
        numeroEdicao: item.numeroEdicao,
        dataPublicacao: item.dataPublicacao,
        thumbCapa: item.thumbCapa,
        capa: item.capa,
        preco: item.preco,
        numeroPaginas: item.numeroPaginas,
        licenciador: item.licenciador,
        personagens: item.personagens,
        desenhosRoteirosArteFinalCores: item.desenhosRoteirosArteFinalCores,
        linkDetalhes: item.linkDetalhes,
        lido: 0
      });
      if (onSuccessImport) {
        onSuccessImport(item);
      }
      handleReset();
    } catch (err) {
      alert('Erro ao importar quadrinho da web.');
    } finally {
      setIsSubmitting(false);
      setImportingItemId(null);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div 
        className="modal-content" 
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 560, maxHeight: '90vh', overflowY: 'auto' }}
      >
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              width: 36,
              height: 36,
              borderRadius: 10,
              background: 'linear-gradient(135deg, var(--primary), var(--accent-pink))',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 4px 12px rgba(99, 102, 241, 0.4)'
            }}>
              <Plus size={20} color="#fff" />
            </div>
            <div>
              <h2 style={{ fontSize: '1.25rem', fontWeight: 800 }}>Adicionar à Coleção</h2>
              <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Cadastre manualmente ou importe da web</span>
            </div>
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'rgba(255, 255, 255, 0.08)',
              border: 'none',
              borderRadius: '50%',
              width: 32,
              height: 32,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--text-muted)',
              cursor: 'pointer'
            }}
          >
            <X size={18} />
          </button>
        </div>

        {/* Tab Switcher */}
        <div style={{
          display: 'flex',
          background: 'rgba(255, 255, 255, 0.04)',
          borderRadius: 'var(--radius-sm)',
          padding: 4,
          marginBottom: 18,
          border: '1px solid var(--border-subtle)'
        }}>
          <button
            type="button"
            onClick={() => setActiveTab('manual')}
            style={{
              flex: 1,
              padding: '8px 12px',
              border: 'none',
              borderRadius: 6,
              background: activeTab === 'manual' ? 'var(--primary)' : 'transparent',
              color: activeTab === 'manual' ? '#fff' : 'var(--text-muted)',
              fontWeight: 700,
              fontSize: '0.85rem',
              cursor: 'pointer',
              transition: 'all 0.2s ease',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 6
            }}
          >
            <Plus size={16} />
            Cadastro Manual
          </button>

          <button
            type="button"
            onClick={() => setActiveTab('web')}
            style={{
              flex: 1,
              padding: '8px 12px',
              border: 'none',
              borderRadius: 6,
              background: activeTab === 'web' ? 'var(--primary)' : 'transparent',
              color: activeTab === 'web' ? '#fff' : 'var(--text-muted)',
              fontWeight: 700,
              fontSize: '0.85rem',
              cursor: 'pointer',
              transition: 'all 0.2s ease',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 6
            }}
          >
            <Globe size={16} />
            Guia dos Quadrinhos (Web)
          </button>
        </div>

        {/* TAB 1: MANUAL FORM */}
        {activeTab === 'manual' && (
          <form onSubmit={handleSubmitManual} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                TÍTULO DA HQ *
              </label>
              <input
                type="text"
                required
                placeholder="Ex: Batman: O Cavaleiro das Trevas"
                value={titulo}
                onChange={(e) => setTitulo(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  EDITORA *
                </label>
                <input
                  type="text"
                  list="add-editoras-list"
                  placeholder="Selecione ou digite"
                  value={editora}
                  onChange={(e) => setEditora(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
                <datalist id="add-editoras-list">
                  {editoras.map(e => <option key={e.id} value={e.nome} />)}
                </datalist>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  NÚMERO DA EDIÇÃO
                </label>
                <input
                  type="number"
                  min="0"
                  value={numeroEdicao}
                  onChange={(e) => setNumeroEdicao(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  PREÇO
                </label>
                <input
                  type="text"
                  placeholder="Ex: 29,90"
                  value={preco}
                  onChange={(e) => setPreco(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  PÁGINAS
                </label>
                <input
                  type="number"
                  min="0"
                  placeholder="Ex: 48"
                  value={numeroPaginas}
                  onChange={(e) => setNumeroPaginas(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  ANO / DATA DE LANÇAMENTO
                </label>
                <input
                  type="text"
                  placeholder="Ex: 2024 ou 2024-05"
                  value={dataPublicacao}
                  onChange={(e) => setDataPublicacao(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  LICENCIADOR (Original)
                </label>
                <input
                  type="text"
                  placeholder="Ex: DC Comics, Marvel"
                  value={licenciador}
                  onChange={(e) => setLicenciador(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>
            </div>

            {/* Cover File Upload Input */}
            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                FOTO DA CAPA (Upload de Arquivo)
              </label>
              <input
                type="file"
                ref={fileInputRef}
                accept="image/png, image/jpeg, image/webp"
                onChange={handleFileChange}
                style={{ display: 'none' }}
              />

              <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  className="btn-secondary"
                  style={{ padding: '8px 14px', fontSize: '0.85rem' }}
                >
                  <Upload size={16} />
                  {selectedCoverFile ? 'Alterar Arquivo' : 'Escolher Imagem...'}
                </button>

                {coverPreview ? (
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <img
                      src={coverPreview}
                      alt="Preview"
                      style={{ width: 36, height: 48, objectFit: 'cover', borderRadius: 4, border: '1px solid var(--border-subtle)' }}
                    />
                    <span style={{ fontSize: '0.8rem', color: 'var(--accent-emerald)', fontWeight: 600 }}>
                      ✓ {selectedCoverFile?.name}
                    </span>
                  </div>
                ) : (
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                    Opcional (JPG, PNG ou WEBP)
                  </span>
                )}
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                PERSONAGENS
              </label>
              <textarea
                rows={2}
                placeholder="Ex: Batman, Robin, Mulher-Gato"
                value={personagens}
                onChange={(e) => setPersonagens(e.target.value)}
                style={{
                  width: '100%',
                  padding: '9px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.88rem',
                  resize: 'vertical'
                }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                EQUIPE CRIATIVA (Roteirista, Desenhista, etc.)
              </label>
              <textarea
                rows={2}
                placeholder="Ex: Frank Miller (Roteiro), Lynn Varley (Cores)"
                value={desenhos}
                onChange={(e) => setDesenhos(e.target.value)}
                style={{
                  width: '100%',
                  padding: '9px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.88rem',
                  resize: 'vertical'
                }}
              />
            </div>

            <div style={{ display: 'flex', gap: 10, marginTop: 10 }}>
              <button
                type="button"
                className="btn-secondary"
                onClick={onClose}
                style={{ flex: 1 }}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="btn-primary"
                disabled={isSubmitting}
                style={{ flex: 2 }}
              >
                {isSubmitting ? <Loader2 size={18} className="animate-spin" /> : <Plus size={18} />}
                Adicionar à Coleção
              </button>
            </div>
          </form>
        )}

        {/* TAB 2: WEB SEARCH (Guia dos Quadrinhos) */}
        {activeTab === 'web' && (
          <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
            <form onSubmit={handleSearchWeb} style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr 0.6fr', gap: 10 }}>
                <div>
                  <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                    TÍTULO OU SÉRIE *
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="Ex: Demolidor, Sandman"
                    value={webQueryTitulo}
                    onChange={(e) => setWebQueryTitulo(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.9rem'
                    }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                    EDITORA (Opcional)
                  </label>
                  <input
                    type="text"
                    placeholder="Ex: Panini, Abril"
                    value={webQueryEditora}
                    onChange={(e) => setWebQueryEditora(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.9rem'
                    }}
                  />
                </div>

                <div>
                  <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                    EDIÇÃO
                  </label>
                  <input
                    type="text"
                    placeholder="Ex: 1"
                    value={webQueryEdicao}
                    onChange={(e) => setWebQueryEdicao(e.target.value)}
                    style={{
                      width: '100%',
                      padding: '10px 12px',
                      background: 'var(--bg-card)',
                      border: '1px solid var(--border-subtle)',
                      borderRadius: 'var(--radius-sm)',
                      color: '#fff',
                      fontSize: '0.9rem'
                    }}
                  />
                </div>
              </div>

              <button
                type="submit"
                className="btn-primary"
                disabled={isSearchingWeb}
                style={{ alignSelf: 'flex-start', padding: '9px 18px', fontSize: '0.88rem' }}
              >
                {isSearchingWeb ? <Loader2 size={16} className="animate-spin" /> : <Search size={16} />}
                Pesquisar no Guia dos Quadrinhos
              </button>
            </form>

            {/* Results List */}
            <div style={{ marginTop: 8 }}>
              {isSearchingWeb && (
                <div style={{ textAlign: 'center', padding: '30px 0', color: 'var(--text-muted)' }}>
                  <Loader2 size={24} className="animate-spin" style={{ margin: '0 auto 8px' }} />
                  <p style={{ fontSize: '0.85rem' }}>Consultando Guia dos Quadrinhos...</p>
                </div>
              )}

              {!isSearchingWeb && hasSearchedWeb && webResults.length === 0 && (
                <div style={{ textAlign: 'center', padding: '24px 0', color: 'var(--text-muted)' }}>
                  <p style={{ fontSize: '0.9rem' }}>Nenhum resultado encontrado para esta busca.</p>
                  <p style={{ fontSize: '0.78rem', marginTop: 4 }}>Tente pesquisar por um título mais curto ou sem acentos.</p>
                </div>
              )}

              {!isSearchingWeb && webResults.length > 0 && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: 10, maxHeight: 380, overflowY: 'auto' }}>
                  <span style={{ fontSize: '0.8rem', color: 'var(--accent-cyan)', fontWeight: 700 }}>
                    {webResults.length} quadrinho(s) encontrado(s):
                  </span>

                  {webResults.map((item, idx) => (
                    <div
                      key={idx}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        padding: 10,
                        background: 'rgba(255, 255, 255, 0.03)',
                        border: '1px solid var(--border-subtle)',
                        borderRadius: 'var(--radius-sm)',
                        gap: 12
                      }}
                    >
                      <div style={{ display: 'flex', alignItems: 'center', gap: 10, flex: 1, minWidth: 0 }}>
                        {item.thumbCapa ? (
                          <img
                            src={item.thumbCapa}
                            alt={item.titulo}
                            referrerPolicy="no-referrer"
                            crossOrigin="anonymous"
                            onError={(e) => {
                              const target = e.currentTarget;
                              target.style.display = 'none';
                              if (target.nextElementSibling) {
                                (target.nextElementSibling as HTMLElement).style.display = 'flex';
                              }
                            }}
                            style={{ width: 36, height: 52, objectFit: 'cover', borderRadius: 4, flexShrink: 0 }}
                          />
                        ) : null}
                        <div
                          style={{
                            width: 36,
                            height: 52,
                            background: 'var(--bg-surface)',
                            borderRadius: 4,
                            flexShrink: 0,
                            display: item.thumbCapa ? 'none' : 'flex',
                            alignItems: 'center',
                            justifyContent: 'center'
                          }}
                        >
                          <BookOpen size={16} color="var(--text-muted)" />
                        </div>

                        <div style={{ minWidth: 0 }}>
                          <h4 style={{ fontSize: '0.88rem', fontWeight: 700, margin: 0, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                            {item.titulo}
                          </h4>
                          <span style={{ fontSize: '0.76rem', color: 'var(--text-muted)' }}>
                            {item.editora} • Edição #{item.numeroEdicao} {item.dataPublicacao ? `• ${item.dataPublicacao}` : ''}
                          </span>
                        </div>
                      </div>

                      {(() => {
                        const currentItemId = item.id || `${item.titulo}-${item.numeroEdicao}-${item.editora}`;
                        const isThisItemImporting = isSubmitting && importingItemId === currentItemId;

                        return (
                          <button
                            type="button"
                            className="btn-secondary"
                            onClick={() => handleImportWebItem(item)}
                            disabled={isSubmitting}
                            style={{
                              padding: '6px 12px',
                              fontSize: '0.8rem',
                              flexShrink: 0,
                              display: 'inline-flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              gap: 6,
                              minWidth: 100,
                              cursor: isSubmitting ? 'not-allowed' : 'pointer',
                              opacity: isSubmitting && !isThisItemImporting ? 0.6 : 1
                            }}
                          >
                            {isThisItemImporting ? (
                              <>
                                <Loader2 size={14} className="animate-spin" />
                                <span>Importando...</span>
                              </>
                            ) : (
                              <>
                                <Check size={14} color="var(--accent-emerald)" />
                                <span>Importar</span>
                              </>
                            )}
                          </button>
                        );
                      })()}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
