import React, { useState, useEffect } from 'react';
import { Heart, Plus, ShoppingCart, Trash2, BookOpen } from 'lucide-react';
import { Desejo } from '../types';
import { apiService } from '../services/api';

export const DesejosPage: React.FC = () => {
  const [desejos, setDesejos] = useState<Desejo[]>([]);
  const [showAddForm, setShowAddForm] = useState(false);
  const [titulo, setTitulo] = useState('');
  const [editora, setEditora] = useState('Panini Comics');
  const [preco, setPreco] = useState('');
  const [capa, setCapa] = useState('');

  useEffect(() => {
    loadDesejos();
  }, []);

  const loadDesejos = async () => {
    try {
      const data = await apiService.getDesejos();
      setDesejos(data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleAddDesejo = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!titulo.trim()) return;

    const created = await apiService.addDesejo({
      titulo: titulo.trim(),
      editora: editora || 'Outros',
      numeroEdicao: 1,
      preco: preco.trim() ? (preco.startsWith('R$') ? preco : `R$ ${preco}`) : undefined,
      thumbCapa: capa.trim() || undefined,
      lido: 0,
    });

    setDesejos(prev => [created, ...prev]);
    setTitulo('');
    setPreco('');
    setCapa('');
    setShowAddForm(false);
  };

  const handleComprei = async (item: Desejo) => {
    // Add to collection
    await apiService.addHQ({
      titulo: item.titulo,
      editora: item.editora,
      numeroEdicao: item.numeroEdicao,
      preco: item.preco,
      thumbCapa: item.thumbCapa,
      personagens: item.personagens,
      desenhosRoteirosArteFinalCores: item.desenhosRoteirosArteFinalCores,
      lido: 0,
    });
    // Remove from wishlist
    await apiService.removeDesejo(item.id);
    setDesejos(prev => prev.filter(d => d.id !== item.id));
    alert(`Parabéns! "${item.titulo}" foi adicionada à sua Coleção!`);
  };

  const handleRemove = async (id: string) => {
    await apiService.removeDesejo(id);
    setDesejos(prev => prev.filter(d => d.id !== id));
  };

  return (
    <div>
      <div style={{ marginBottom: 24, display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16 }}>
        <div>
          <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--accent-pink)', letterSpacing: 1, textTransform: 'uppercase' }}>
            Planejamento de Compras
          </span>
          <h1 style={{ fontSize: '2rem', fontWeight: 800, marginTop: 2 }}>
            Lista de Desejos
          </h1>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            Quadrinhos e encadernados que você está de olho para adquirir.
          </p>
        </div>

        <button
          type="button"
          className="btn-primary"
          style={{ background: 'linear-gradient(135deg, var(--accent-pink), #f43f5e)' }}
          onClick={() => setShowAddForm(!showAddForm)}
        >
          <Plus size={18} />
          <span>{showAddForm ? 'Cancelar' : 'Adicionar Desejo'}</span>
        </button>
      </div>

      {/* Add Form Drawer */}
      {showAddForm && (
        <form
          onSubmit={handleAddDesejo}
          style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 'var(--radius-md)',
            padding: 20,
            marginBottom: 24,
            display: 'flex',
            flexDirection: 'column',
            gap: 14,
            boxShadow: '0 8px 24px rgba(0,0,0,0.4)'
          }}
        >
          <h3 style={{ fontSize: '1.1rem', fontWeight: 700 }}>Novo Item de Desejo</h3>

          <div>
            <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
              TÍTULO
            </label>
            <input
              type="text"
              required
              placeholder="Ex: Demolidor por Brian Michael Bendis Vol 1"
              value={titulo}
              onChange={(e) => setTitulo(e.target.value)}
              style={{
                width: '100%',
                padding: '10px 14px',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: '#fff',
                fontSize: '0.95rem'
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
                placeholder="Ex: Panini"
                value={editora}
                onChange={(e) => setEditora(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px 12px',
                  background: 'var(--bg-surface)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
                PREÇO ESTIMADO
              </label>
              <input
                type="text"
                placeholder="Ex: 89,90"
                value={preco}
                onChange={(e) => setPreco(e.target.value)}
                style={{
                  width: '100%',
                  padding: '10px 12px',
                  background: 'var(--bg-surface)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 'var(--radius-sm)',
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              />
            </div>
          </div>

          <div>
            <label style={{ display: 'block', fontSize: '0.78rem', fontWeight: 700, marginBottom: 4, color: 'var(--text-muted)' }}>
              URL DA CAPA
            </label>
            <input
              type="url"
              placeholder="https://..."
              value={capa}
              onChange={(e) => setCapa(e.target.value)}
              style={{
                width: '100%',
                padding: '10px 14px',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: '#fff',
                fontSize: '0.9rem'
              }}
            />
          </div>

          <button type="submit" className="btn-primary" style={{ marginTop: 4 }}>
            Salvar Desejo
          </button>
        </form>
      )}

      {/* Grid of Wishlist Items */}
      {desejos.length > 0 ? (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 16 }}>
          {desejos.map((item) => (
            <div
              key={item.id}
              style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-md)',
                padding: 16,
                display: 'flex',
                gap: 14,
                position: 'relative'
              }}
            >
              {item.thumbCapa ? (
                <img
                  src={item.thumbCapa}
                  alt={item.titulo}
                  style={{
                    width: 70,
                    aspectRatio: '2/3',
                    objectFit: 'cover',
                    borderRadius: 'var(--radius-sm)',
                    flexShrink: 0
                  }}
                />
              ) : (
                <div style={{
                  width: 70,
                  aspectRatio: '2/3',
                  background: '#111827',
                  borderRadius: 'var(--radius-sm)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  flexShrink: 0
                }}>
                  <BookOpen size={24} color="var(--text-dim)" />
                </div>
              )}

              <div style={{ display: 'flex', flexDirection: 'column', flex: 1, minWidth: 0 }}>
                <span style={{ fontSize: '0.7rem', fontWeight: 800, textTransform: 'uppercase', color: 'var(--accent-pink)' }}>
                  {item.editora}
                </span>
                <h4 style={{
                  fontSize: '0.95rem',
                  fontWeight: 700,
                  marginTop: 2,
                  marginBottom: 6,
                  whiteSpace: 'nowrap',
                  overflow: 'hidden',
                  textOverflow: 'ellipsis'
                }}>
                  {item.titulo}
                </h4>

                {item.preco && (
                  <span style={{ fontSize: '0.88rem', fontWeight: 800, color: 'var(--accent-amber)', marginBottom: 'auto' }}>
                    {item.preco}
                  </span>
                )}

                <div style={{ display: 'flex', gap: 8, marginTop: 12 }}>
                  <button
                    type="button"
                    onClick={() => handleComprei(item)}
                    style={{
                      background: 'rgba(16, 185, 129, 0.15)',
                      border: '1px solid rgba(16, 185, 129, 0.4)',
                      color: 'var(--accent-emerald)',
                      padding: '6px 12px',
                      borderRadius: 'var(--radius-sm)',
                      fontSize: '0.75rem',
                      fontWeight: 700,
                      cursor: 'pointer',
                      display: 'flex',
                      alignItems: 'center',
                      gap: 4
                    }}
                  >
                    <ShoppingCart size={14} />
                    Comprei!
                  </button>

                  <button
                    type="button"
                    onClick={() => handleRemove(item.id)}
                    style={{
                      background: 'transparent',
                      border: 'none',
                      color: 'var(--danger)',
                      padding: '6px 8px',
                      cursor: 'pointer'
                    }}
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div style={{ textAlign: 'center', padding: '60px 20px', background: 'var(--bg-card)', borderRadius: 'var(--radius-md)' }}>
          <Heart size={44} color="var(--text-dim)" style={{ margin: '0 auto 14px' }} />
          <h3 style={{ fontSize: '1.2rem', fontWeight: 700 }}>Sua lista de desejos está vazia</h3>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
            Adicione as próximas HQs que você pretende comprar.
          </p>
        </div>
      )}
    </div>
  );
};
