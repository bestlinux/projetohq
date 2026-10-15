import { useState } from 'react';
import { Check, BookOpen, Maximize2 } from 'lucide-react';
import { HQ } from '../types';
import { getCapaUrl } from '../services/api';

interface HQCardProps {
  hq: HQ;
  onSelect: (hq: HQ) => void;
  onToggleRead: (e: React.MouseEvent, hqId: string) => void;
  onExpand?: (hq: HQ) => void;
}

export const HQCard: React.FC<HQCardProps> = ({ hq, onSelect, onToggleRead, onExpand }) => {
  const [imgError, setImgError] = useState(false);
  const isRead = hq.lido === 1;

  const coverSrc = getCapaUrl(hq.capa) || hq.thumbCapa;
  const showPlaceholder = !coverSrc || imgError;

  return (
    <article className="hq-card" onClick={() => onSelect(hq)}>
      <div className="cover-wrapper">
        {!showPlaceholder && (
          <img
            src={coverSrc}
            alt={hq.titulo}
            className="cover-img"
            loading="lazy"
            onError={() => setImgError(true)}
          />
        )}

        {showPlaceholder && (
          <div className="cover-placeholder" style={{
            background: 'linear-gradient(135deg, rgba(30, 41, 59, 0.95), rgba(15, 23, 42, 0.98))',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            padding: 12,
            textAlign: 'center',
            gap: 8,
            height: '100%',
            width: '100%',
            border: '1px solid rgba(255,255,255,0.06)'
          }}>
            <div style={{
              width: 38,
              height: 38,
              borderRadius: 8,
              background: 'rgba(99, 102, 241, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <BookOpen size={20} color="var(--primary)" />
            </div>
            <span style={{
              fontSize: '0.74rem',
              fontWeight: 700,
              color: 'var(--text-main)',
              display: '-webkit-box',
              WebkitLineClamp: 3,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              lineHeight: 1.3
            }}>
              {hq.titulo}
            </span>
            <span style={{ fontSize: '0.68rem', color: 'var(--text-dim)', textTransform: 'uppercase', letterSpacing: 0.5 }}>
              {hq.editora || 'HQ'}
            </span>
          </div>
        )}

        {/* Edition Number Badge */}
        <div className="badge-edition">
          #{hq.numeroEdicao > 0 ? hq.numeroEdicao : '1'}
        </div>

        {/* Read Status Toggle Button */}
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            onToggleRead(e, hq.id);
          }}
          className="badge-read"
          title={isRead ? 'HQ Lida (toque para desmarcar)' : 'Marcar como lida'}
          style={{
            background: isRead ? 'rgba(16, 185, 129, 0.95)' : 'rgba(15, 23, 42, 0.75)',
            border: isRead ? 'none' : '1px solid rgba(255, 255, 255, 0.2)',
            color: isRead ? '#fff' : 'var(--text-muted)'
          }}
        >
          {isRead ? (
            <>
              <Check size={12} strokeWidth={3} />
              <span>Lida</span>
            </>
          ) : (
            <span>Não lida</span>
          )}
        </button>

        {/* Expand Cover Button */}
        {onExpand && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              onExpand(hq);
            }}
            className="badge-expand"
            title="Expandir capa da HQ"
            style={{
              position: 'absolute',
              bottom: 8,
              right: 8,
              width: 30,
              height: 30,
              borderRadius: 8,
              background: 'rgba(0, 0, 0, 0.72)',
              backdropFilter: 'blur(6px)',
              border: '1px solid rgba(255, 255, 255, 0.25)',
              color: '#fff',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              boxShadow: '0 4px 10px rgba(0,0,0,0.4)',
              transition: 'all 0.2s ease',
              zIndex: 10
            }}
          >
            <Maximize2 size={14} />
          </button>
        )}
      </div>

      <div className="hq-info">
        <span className="hq-publisher">{hq.editora || 'Outros'}</span>
        <h3 className="hq-title">{hq.titulo}</h3>

        <div className="hq-footer">
          <span>{hq.dataPublicacao || (hq.numeroPaginas ? `${hq.numeroPaginas} págs` : 'Edição única')}</span>
          {hq.preco && <span className="hq-price">{hq.preco}</span>}
        </div>
      </div>
    </article>
  );
};
