import React, { useState, useEffect } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { BookOpen, Building2, Heart, CheckCircle2, Settings, Sparkles, Menu, X, ChevronRight, Database, Quote } from 'lucide-react';

export const Navbar: React.FC = () => {
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);
  const location = useLocation();

  // Fecha a gaveta lateral sempre que a rota mudar
  useEffect(() => {
    setIsDrawerOpen(false);
  }, [location.pathname]);

  // Fecha com a tecla ESC
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setIsDrawerOpen(false);
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Evita scroll da página ao fundo quando a gaveta estiver aberta
  useEffect(() => {
    if (isDrawerOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
  }, [isDrawerOpen]);

  const navItems = [
    {
      to: '/',
      label: 'Coleção',
      subtitle: 'Catálogo de quadrinhos',
      icon: BookOpen,
      end: true
    },
    {
      to: '/editoras',
      label: 'Editoras',
      subtitle: 'Selos e publicadoras',
      icon: Building2,
      end: false
    },
    {
      to: '/desejos',
      label: 'Desejos',
      subtitle: 'Lista de compras',
      icon: Heart,
      end: false
    },
    {
      to: '/leituras',
      label: 'Leituras',
      subtitle: 'Fila e histórico de leitura',
      icon: CheckCircle2,
      end: false
    },
    {
      to: '/frases',
      label: 'Frases',
      subtitle: 'Painéis e citações memoráveis',
      icon: Quote,
      end: false
    },
    {
      to: '/configuracoes',
      label: 'Conexão / PWA',
      subtitle: 'Status da API e banco',
      icon: Settings,
      end: false
    }
  ];

  return (
    <>
      {/* Top Header Bar */}
      <header className="desktop-header" style={{ display: 'block' }}>
        <div className="header-container" style={{ display: 'flex', alignItems: 'center' }}>
          <button
            type="button"
            onClick={() => setIsDrawerOpen(true)}
            title="Abrir Menu Lateral"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 10,
              padding: '10px 18px',
              borderRadius: 'var(--radius-sm)',
              background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.16), rgba(255, 255, 255, 0.05))',
              border: '1px solid rgba(99, 102, 241, 0.35)',
              color: 'var(--text-main)',
              fontSize: '0.92rem',
              fontWeight: 700,
              cursor: 'pointer',
              transition: 'all 0.2s ease',
              boxShadow: '0 2px 10px rgba(0, 0, 0, 0.25)'
            }}
          >
            <Menu size={20} color="var(--primary)" />
            <span>Abrir Menu Lateral</span>
          </button>
        </div>
      </header>

      {/* Side Drawer (Gaveta Lateral) Backdrop */}
      {isDrawerOpen && (
        <div
          className="drawer-backdrop"
          onClick={() => setIsDrawerOpen(false)}
        />
      )}

      {/* Side Drawer (Gaveta Lateral) */}
      <aside
        className="side-drawer"
        style={{
          transform: isDrawerOpen ? 'translateX(0)' : 'translateX(-100%)',
          transition: 'transform 0.28s cubic-bezier(0.16, 1, 0.3, 1)'
        }}
      >
        {/* Drawer Header */}
        <div className="drawer-header">
          <NavLink to="/" className="brand" onClick={() => setIsDrawerOpen(false)}>
            <div style={{
              width: 34,
              height: 34,
              borderRadius: 10,
              background: 'linear-gradient(135deg, #6366f1, #ec4899)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 0 12px rgba(99, 102, 241, 0.4)'
            }}>
              <Sparkles size={18} color="#fff" />
            </div>
            <span style={{ fontSize: '1.25rem' }}>PROJETO <span className="brand-badge" style={{ fontSize: '1.4rem' }}>HQ</span></span>
          </NavLink>

          <button
            type="button"
            onClick={() => setIsDrawerOpen(false)}
            title="Fechar menu"
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              padding: 6,
              borderRadius: 8,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            <X size={20} />
          </button>
        </div>

        {/* Navigation list */}
        <nav className="drawer-nav">
          <div style={{ padding: '0 8px 6px', fontSize: '0.72rem', fontWeight: 700, color: 'var(--text-dim)', textTransform: 'uppercase', letterSpacing: 1 }}>
            Navegação Principal
          </div>

          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                onClick={() => setIsDrawerOpen(false)}
                className={({ isActive }) => `drawer-item ${isActive ? 'active' : ''}`}
              >
                <div className="drawer-item-icon">
                  <Icon size={19} />
                </div>
                <div style={{ flex: 1, minWidth: 0 }}>
                  <div style={{ fontSize: '0.94rem', fontWeight: 700 }}>
                    {item.label}
                  </div>
                  <div style={{ fontSize: '0.74rem', color: 'var(--text-dim)', marginTop: 1 }}>
                    {item.subtitle}
                  </div>
                </div>
                <ChevronRight size={16} color="var(--text-dim)" />
              </NavLink>
            );
          })}
        </nav>

        {/* Drawer Footer */}
        <div className="drawer-footer">
          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: 10,
            padding: '10px 12px',
            background: 'rgba(16, 185, 129, 0.08)',
            border: '1px solid rgba(16, 185, 129, 0.22)',
            borderRadius: 'var(--radius-sm)'
          }}>
            <Database size={16} color="var(--accent-emerald)" />
            <div>
              <div style={{ fontSize: '0.78rem', fontWeight: 700, color: 'var(--accent-emerald)' }}>
                SQL Server Conectado
              </div>
              <div style={{ fontSize: '0.7rem', color: 'var(--text-muted)' }}>
                Servidor: GROGU • v.NET 10
              </div>
            </div>
          </div>
        </div>
      </aside>

      {/* Mobile Bottom Navigation Bar (com acesso rápido e gaveta) */}
      <nav className="mobile-nav">
        <NavLink to="/" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`} end>
          <BookOpen size={20} />
          <span>Coleção</span>
        </NavLink>
        <NavLink to="/editoras" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
          <Building2 size={20} />
          <span>Editoras</span>
        </NavLink>
        <NavLink to="/desejos" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
          <Heart size={20} />
          <span>Desejos</span>
        </NavLink>
        <NavLink to="/leituras" className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}>
          <CheckCircle2 size={20} />
          <span>Leituras</span>
        </NavLink>
        <button
          type="button"
          onClick={() => setIsDrawerOpen(true)}
          className="nav-item"
          style={{ background: 'transparent', border: 'none', cursor: 'pointer' }}
        >
          <Menu size={20} />
          <span>Menu</span>
        </button>
      </nav>
    </>
  );
};
