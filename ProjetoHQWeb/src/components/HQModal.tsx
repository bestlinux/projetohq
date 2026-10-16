import React, { useState, useRef, useEffect } from 'react';
import { 
  X, CheckCircle, Trash2, Heart, Calendar, Book, DollarSign, 
  Users, Feather, Edit3, Camera, Save, RotateCcw, Loader2 
} from 'lucide-react';
import { HQ, Editora } from '../types';
import { apiService, getCapaUrl } from '../services/api';

interface HQModalProps {
  hq: HQ | null;
  editoras?: Editora[];
  onClose: () => void;
  onToggleRead: (hqId: string) => void;
  onRemove: (hqId: string) => void;
  onUpdate?: (updatedHQ: HQ) => void;
  onMoveToWishlist?: (hq: HQ) => void;
}

export const HQModal: React.FC<HQModalProps> = ({
  hq,
  editoras = [],
  onClose,
  onToggleRead,
  onRemove,
  onUpdate,
  onMoveToWishlist,
}) => {
  const [isEditing, setIsEditing] = useState(false);
  const [isUploadingCover, setIsUploadingCover] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Form states for editing
  const [formTitulo, setFormTitulo] = useState('');
  const [formEditora, setFormEditora] = useState('');
  const [formNumeroEdicao, setFormNumeroEdicao] = useState(1);
  const [formPreco, setFormPreco] = useState('');
  const [formNumeroPaginas, setFormNumeroPaginas] = useState(0);
  const [formDataPublicacao, setFormDataPublicacao] = useState('');
  const [formLicenciador, setFormLicenciador] = useState('');
  const [formPersonagens, setFormPersonagens] = useState('');
  const [formEquipe, setFormEquipe] = useState('');

  // Reset form when hq changes or edit mode opens
  useEffect(() => {
    if (hq) {
      setFormTitulo(hq.titulo || '');
      setFormEditora(hq.editora || '');
      setFormNumeroEdicao(hq.numeroEdicao || 1);
      setFormPreco(hq.preco || '');
      setFormNumeroPaginas(hq.numeroPaginas || 0);
      setFormDataPublicacao(hq.dataPublicacao || '');
      setFormLicenciador(hq.licenciador || '');
      setFormPersonagens(hq.personagens || '');
      setFormEquipe(hq.desenhosRoteirosArteFinalCores || '');
      setIsEditing(false);
    }
  }, [hq]);

  if (!hq) return null;

  const isRead = hq.lido === 1;
  const coverSrc = getCapaUrl(hq.capa) || hq.thumbCapa;

  // Handle Cover Upload
  const handleCoverFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setIsUploadingCover(true);
    try {
      const res = await apiService.uploadCapa(hq.id, file);
      const updated: HQ = {
        ...hq,
        capa: res.capa,
        thumbCapa: res.url
      };
      if (onUpdate) {
        onUpdate(updated);
      }
    } catch (err: any) {
      alert(err.message || 'Erro ao fazer upload da capa.');
    } finally {
      setIsUploadingCover(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  // Handle Save Edit
  const handleSaveEdit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formTitulo.trim()) return;

    setIsSaving(true);
    try {
      const updatedHQ: HQ = {
        ...hq,
        titulo: formTitulo.trim(),
        editora: formEditora.trim() || 'Independente',
        numeroEdicao: Number(formNumeroEdicao) || 1,
        preco: formPreco.trim(),
        numeroPaginas: Number(formNumeroPaginas) || 0,
        dataPublicacao: formDataPublicacao.trim(),
        licenciador: formLicenciador.trim(),
        personagens: formPersonagens.trim(),
        desenhosRoteirosArteFinalCores: formEquipe.trim()
      };

      await apiService.updateHQ(updatedHQ);
      if (onUpdate) {
        onUpdate(updatedHQ);
      }
      setIsEditing(false);
    } catch (err) {
      alert('Erro ao salvar edições da HQ.');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div 
        className="modal-content" 
        onClick={(e) => e.stopPropagation()}
        style={{ maxWidth: 540, maxHeight: '90vh', overflowY: 'auto' }}
      >
        {/* Top bar with Edit Toggle and Close */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
          <div>
            <span style={{
              fontSize: '0.75rem',
              fontWeight: 800,
              textTransform: 'uppercase',
              color: 'var(--accent-pink)',
              letterSpacing: 1
            }}>
              {hq.editora} {hq.licenciador ? `• ${hq.licenciador}` : ''}
            </span>
            <h2 style={{ fontSize: '1.35rem', fontWeight: 800, marginTop: 4, lineHeight: 1.2 }}>
              {isEditing ? 'Editar Quadrinho' : hq.titulo}
            </h2>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <button
              type="button"
              onClick={() => setIsEditing(!isEditing)}
              title={isEditing ? 'Cancelar edição' : 'Editar dados da HQ'}
              style={{
                background: isEditing ? 'rgba(236, 72, 153, 0.18)' : 'rgba(255, 255, 255, 0.08)',
                border: isEditing ? '1px solid var(--accent-pink)' : '1px solid var(--border-subtle)',
                borderRadius: '50%',
                width: 36,
                height: 36,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: isEditing ? 'var(--accent-pink)' : 'var(--text-main)',
                cursor: 'pointer',
                transition: 'all 0.2s ease'
              }}
            >
              {isEditing ? <RotateCcw size={17} /> : <Edit3 size={17} />}
            </button>

            <button
              onClick={onClose}
              style={{
                background: 'rgba(255, 255, 255, 0.08)',
                border: 'none',
                borderRadius: '50%',
                width: 36,
                height: 36,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'var(--text-muted)',
                cursor: 'pointer'
              }}
            >
              <X size={20} />
            </button>
          </div>
        </div>

        {/* Hidden file input for cover upload */}
        <input 
          type="file" 
          ref={fileInputRef} 
          onChange={handleCoverFileChange} 
          accept="image/png, image/jpeg, image/webp" 
          style={{ display: 'none' }} 
        />

        {/* Content Section: VIEW MODE vs EDIT MODE */}
        {!isEditing ? (
          <>
            {/* Cover and Quick Specs */}
            <div style={{ display: 'flex', flexDirection: 'row', gap: 18, marginBottom: 20 }}>
              <div style={{ position: 'relative', width: 125, flexShrink: 0 }}>
                <div style={{
                  width: '100%',
                  aspectRatio: '2/3',
                  borderRadius: 'var(--radius-sm)',
                  overflow: 'hidden',
                  boxShadow: '0 8px 18px rgba(0,0,0,0.5)',
                  border: '1px solid var(--border-subtle)',
                  background: 'var(--bg-surface)'
                }}>
                  {coverSrc ? (
                    <img
                      src={coverSrc}
                      alt={hq.titulo}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    />
                  ) : (
                    <div style={{
                      width: '100%',
                      height: '100%',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: '0.75rem',
                      color: 'var(--text-muted)',
                      textAlign: 'center',
                      padding: 8
                    }}>
                      Sem Capa
                    </div>
                  )}
                </div>

                {/* Upload Cover Button Overlay */}
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isUploadingCover}
                  title="Alterar imagem de capa"
                  style={{
                    position: 'absolute',
                    bottom: 6,
                    right: 6,
                    background: 'rgba(0, 0, 0, 0.75)',
                    backdropFilter: 'blur(4px)',
                    border: '1px solid rgba(255, 255, 255, 0.2)',
                    borderRadius: '50%',
                    width: 32,
                    height: 32,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: '#fff',
                    cursor: 'pointer',
                    boxShadow: '0 2px 8px rgba(0,0,0,0.4)'
                  }}
                >
                  {isUploadingCover ? <Loader2 size={16} className="animate-spin" /> : <Camera size={16} />}
                </button>
              </div>

              <div style={{ display: 'flex', flexDirection: 'column', gap: 10, flex: 1, fontSize: '0.85rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--text-muted)' }}>
                  <Book size={16} color="var(--primary)" />
                  <span>Edição: <strong style={{ color: '#fff' }}>#{hq.numeroEdicao}</strong></span>
                </div>

                {hq.numeroPaginas ? (
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--text-muted)' }}>
                    <Book size={16} color="var(--accent-cyan)" />
                    <span>Páginas: <strong style={{ color: '#fff' }}>{hq.numeroPaginas}</strong></span>
                  </div>
                ) : null}

                {hq.dataPublicacao && (
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--text-muted)' }}>
                    <Calendar size={16} color="var(--accent-amber)" />
                    <span>Publicação: <strong style={{ color: '#fff' }}>{hq.dataPublicacao}</strong></span>
                  </div>
                )}

                {hq.preco && (
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--text-muted)' }}>
                    <DollarSign size={16} color="var(--accent-emerald)" />
                    <span>Preço: <strong style={{ color: 'var(--accent-amber)' }}>{hq.preco}</strong></span>
                  </div>
                )}

                {hq.capa && (
                  <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', wordBreak: 'break-all', marginTop: 4 }}>
                    Arquivo: <span style={{ fontFamily: 'monospace' }}>{hq.capa}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Detailed Info Sections */}
            {hq.personagens && (
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                padding: 12,
                borderRadius: 'var(--radius-sm)',
                marginBottom: 12,
                border: '1px solid var(--border-subtle)'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 4, color: 'var(--primary)', fontWeight: 700, fontSize: '0.8rem' }}>
                  <Users size={14} />
                  <span>PERSONAGENS</span>
                </div>
                <p style={{ fontSize: '0.86rem', color: 'var(--text-main)', lineHeight: 1.4 }}>
                  {hq.personagens}
                </p>
              </div>
            )}

            {hq.desenhosRoteirosArteFinalCores && (
              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                padding: 12,
                borderRadius: 'var(--radius-sm)',
                marginBottom: 20,
                border: '1px solid var(--border-subtle)'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 6, marginBottom: 4, color: 'var(--accent-pink)', fontWeight: 700, fontSize: '0.8rem' }}>
                  <Feather size={14} />
                  <span>EQUIPE CRIATIVA</span>
                </div>
                <p style={{ fontSize: '0.84rem', color: 'var(--text-muted)', lineHeight: 1.4 }}>
                  {hq.desenhosRoteirosArteFinalCores}
                </p>
              </div>
            )}

            {/* Action Buttons */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              <button
                type="button"
                className={isRead ? 'btn-secondary' : 'btn-primary'}
                onClick={() => onToggleRead(hq.id)}
                style={{ width: '100%' }}
              >
                <CheckCircle size={18} />
                {isRead ? 'Marcar como Não Lido' : 'Marcar como Lido'}
              </button>

              <button
                type="button"
                className="btn-secondary"
                onClick={() => setIsEditing(true)}
                style={{ width: '100%' }}
              >
                <Edit3 size={18} color="var(--primary)" />
                Editar Dados da HQ
              </button>

              {onMoveToWishlist && (
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => onMoveToWishlist(hq)}
                  style={{ width: '100%' }}
                >
                  <Heart size={18} color="var(--accent-pink)" />
                  Mover para Lista de Desejos
                </button>
              )}

              <button
                type="button"
                onClick={() => {
                  if (window.confirm(`Tem certeza que deseja remover "${hq.titulo}"?`)) {
                    onRemove(hq.id);
                    onClose();
                  }
                }}
                style={{
                  background: 'transparent',
                  color: 'var(--danger)',
                  border: 'none',
                  padding: '10px',
                  fontSize: '0.88rem',
                  fontWeight: 600,
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: 6
                }}
              >
                <Trash2 size={16} />
                Remover da Coleção
              </button>
            </div>
          </>
        ) : (
          /* EDIT FORM */
          <form onSubmit={handleSaveEdit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                TÍTULO *
              </label>
              <input
                type="text"
                required
                value={formTitulo}
                onChange={(e) => setFormTitulo(e.target.value)}
                style={{
                  width: '100%',
                  padding: '9px 12px',
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
                  EDITORA
                </label>
                <input
                  type="text"
                  list="editoras-list"
                  value={formEditora}
                  onChange={(e) => setFormEditora(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
                <datalist id="editoras-list">
                  {editoras.map(e => <option key={e.id} value={e.nome} />)}
                </datalist>
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                  Nº EDIÇÃO
                </label>
                <input
                  type="number"
                  min="0"
                  value={formNumeroEdicao}
                  onChange={(e) => setFormNumeroEdicao(parseInt(e.target.value) || 1)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
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
                  placeholder="Ex: R$ 29,90"
                  value={formPreco}
                  onChange={(e) => setFormPreco(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
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
                  Nº DE PÁGINAS
                </label>
                <input
                  type="number"
                  min="0"
                  value={formNumeroPaginas}
                  onChange={(e) => setFormNumeroPaginas(parseInt(e.target.value) || 0)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
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
                  DATA / ANO
                </label>
                <input
                  type="text"
                  placeholder="Ex: 2024 ou 2024-05"
                  value={formDataPublicacao}
                  onChange={(e) => setFormDataPublicacao(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
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
                  LICENCIADOR
                </label>
                <input
                  type="text"
                  placeholder="Ex: DC Comics, Marvel"
                  value={formLicenciador}
                  onChange={(e) => setFormLicenciador(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '9px 12px',
                    background: 'var(--bg-card)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 'var(--radius-sm)',
                    color: '#fff',
                    fontSize: '0.88rem'
                  }}
                />
              </div>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                PERSONAGENS
              </label>
              <textarea
                rows={2}
                placeholder="Ex: Batman, Coringa, Robin"
                value={formPersonagens}
                onChange={(e) => setFormPersonagens(e.target.value)}
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
                EQUIPE CRIATIVA (Desenhos, Roteiro, etc.)
              </label>
              <textarea
                rows={2}
                placeholder="Ex: Frank Miller (Roteiro), Klaus Janson (Arte)"
                value={formEquipe}
                onChange={(e) => setFormEquipe(e.target.value)}
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

            {/* Save / Cancel buttons */}
            <div style={{ display: 'flex', gap: 10, marginTop: 10 }}>
              <button
                type="button"
                className="btn-secondary"
                onClick={() => setIsEditing(false)}
                style={{ flex: 1 }}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="btn-primary"
                disabled={isSaving}
                style={{ flex: 2 }}
              >
                {isSaving ? <Loader2 size={18} className="animate-spin" /> : <Save size={18} />}
                Salvar Alterações
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
