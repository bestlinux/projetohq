import React, { useState } from 'react';
import { X, Plus } from 'lucide-react';
import { HQ, Editora } from '../types';

interface AddHQModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAdd: (hq: Omit<HQ, 'id'>) => void;
  editoras: Editora[];
}

export const AddHQModal: React.FC<AddHQModalProps> = ({ isOpen, onClose, onAdd, editoras }) => {
  const [titulo, setTitulo] = useState('');
  const [editora, setEditora] = useState(editoras[0]?.nome || 'Panini Comics');
  const [numeroEdicao, setNumeroEdicao] = useState('1');
  const [preco, setPreco] = useState('');
  const [thumbCapa, setThumbCapa] = useState('');
  const [personagens, setPersonagens] = useState('');
  const [desenhos, setDesenhos] = useState('');

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) return;

    onAdd({
      titulo: titulo.trim(),
      editora: editora || 'Outros',
      numeroEdicao: parseInt(numeroEdicao, 10) || 1,
      preco: preco.trim() ? (preco.startsWith('R$') ? preco : `R$ ${preco}`) : undefined,
      thumbCapa: thumbCapa.trim() || undefined,
      personagens: personagens.trim() || undefined,
      desenhosRoteirosArteFinalCores: desenhos.trim() || undefined,
      lido: 0,
    });

    // Reset
    setTitulo('');
    setThumbCapa('');
    setPreco('');
    setPersonagens('');
    setDesenhos('');
    onClose();
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <div style={{
              width: 34,
              height: 34,
              borderRadius: 8,
              background: 'linear-gradient(135deg, var(--primary), var(--accent-pink))',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <Plus size={20} color="#fff" />
            </div>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 800 }}>Adicionar à Coleção</h2>
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

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
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
                padding: '11px 14px',
                background: 'var(--bg-card)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: '#fff',
                fontSize: '0.95rem'
              }}
            />
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
                EDITORA
              </label>
              <select
                value={editora}
                onChange={(e) => setEditora(e.target.value)}
                style={{
                  width: '100%',
                  padding: '11px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              >
                {editoras.map((ed) => (
                  <option key={ed.id} value={ed.nome} style={{ background: '#111827' }}>
                    {ed.nome}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
                EDIÇÃO Nº
              </label>
              <input
                type="number"
                min="0"
                value={numeroEdicao}
                onChange={(e) => setNumeroEdicao(e.target.value)}
                style={{
                  width: '100%',
                  padding: '11px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
                PREÇO ESTIMADO
              </label>
              <input
                type="text"
                placeholder="Ex: 59,90"
                value={preco}
                onChange={(e) => setPreco(e.target.value)}
                style={{
                  width: '100%',
                  padding: '11px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
                URL DA CAPA (IMAGEM)
              </label>
              <input
                type="url"
                placeholder="https://..."
                value={thumbCapa}
                onChange={(e) => setThumbCapa(e.target.value)}
                style={{
                  width: '100%',
                  padding: '11px 12px',
                  background: 'var(--bg-card)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>
          </div>

          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
              PERSONAGENS
            </label>
            <input
              type="text"
              placeholder="Ex: Batman, Coringa, Robin"
              value={personagens}
              onChange={(e) => setPersonagens(e.target.value)}
              style={{
                width: '100%',
                padding: '11px 14px',
                background: 'var(--bg-card)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: '#fff',
                fontSize: '0.9rem'
              }}
            />
          </div>

          <button type="submit" className="btn-primary" style={{ marginTop: 8 }}>
            <Plus size={18} />
            Salvar HQ
          </button>
        </form>
      </div>
    </div>
  );
};
