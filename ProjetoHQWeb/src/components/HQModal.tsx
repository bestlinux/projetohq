import React from 'react';
import { X, CheckCircle, Trash2, Heart, Calendar, Book, DollarSign, Users, Feather } from 'lucide-react';
import { HQ } from '../types';
import { getCapaUrl } from '../services/api';

interface HQModalProps {
  hq: HQ | null;
  onClose: () => void;
  onToggleRead: (hqId: string) => void;
  onRemove: (hqId: string) => void;
  onMoveToWishlist?: (hq: HQ) => void;
}

export const HQModal: React.FC<HQModalProps> = ({
  hq,
  onClose,
  onToggleRead,
  onRemove,
  onMoveToWishlist,
}) => {
  if (!hq) return null;

  const isRead = hq.lido === 1;

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
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
              {hq.titulo}
            </h2>
          </div>
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

        {/* Content with cover and details */}
        {(() => {
          const coverSrc = getCapaUrl(hq.capa) || hq.thumbCapa;
          return (
            <div style={{ display: 'flex', flexDirection: 'row', gap: 18, marginBottom: 20 }}>
              {coverSrc && (
                <div style={{
                  width: 120,
                  aspectRatio: '2/3',
                  borderRadius: 'var(--radius-sm)',
                  overflow: 'hidden',
                  flexShrink: 0,
                  boxShadow: '0 8px 16px rgba(0,0,0,0.5)',
                  border: '1px solid var(--border-subtle)'
                }}>
                  <img
                    src={coverSrc}
                    alt={hq.titulo}
                    style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                  />
                </div>
              )}

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
          </div>
        </div>
      );
    })()}

        {/* Detailed Sections */}
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
      </div>
    </div>
  );
};
