import React, { useState, useEffect } from 'react';
import { Smartphone, Server, RefreshCw, CheckCircle, AlertCircle } from 'lucide-react';
import { getStoredApiUrl, setStoredApiUrl, apiService } from '../services/api';

export const SettingsPage: React.FC = () => {
  const [apiUrl, setApiUrl] = useState(getStoredApiUrl());
  const [pingStatus, setPingStatus] = useState<{ checked: boolean; success: boolean; message: string }>({
    checked: false,
    success: false,
    message: ''
  });
  const [checking, setChecking] = useState(false);

  useEffect(() => {
    testConnection();
  }, []);

  const testConnection = async () => {
    setChecking(true);
    const result = await apiService.pingBackend();
    setPingStatus({
      checked: true,
      success: result.success,
      message: result.message
    });
    setChecking(false);
  };

  const handleSaveUrl = (e: React.FormEvent) => {
    e.preventDefault();
    setStoredApiUrl(apiUrl.trim());
    testConnection();
    alert('URL da API salva com sucesso!');
  };

  const handleResetData = () => {
    if (window.confirm('Deseja resetar o cache local para os dados padrão?')) {
      localStorage.clear();
      window.location.reload();
    }
  };

  return (
    <div style={{ maxWidth: 760, margin: '0 auto' }}>
      <div style={{ marginBottom: 24 }}>
        <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--primary)', letterSpacing: 1, textTransform: 'uppercase' }}>
          Configurações & PWA
        </span>
        <h1 style={{ fontSize: '2rem', fontWeight: 800, marginTop: 2 }}>
          Conexão & Celular
        </h1>
        <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', marginTop: 4 }}>
          Gerencie o acesso mobile (PWA) e a comunicação com a API .NET 10.
        </p>
      </div>

      {/* Mobile PWA Card */}
      <div style={{
        background: 'linear-gradient(135deg, rgba(99, 102, 241, 0.14), rgba(24, 34, 53, 0.95))',
        border: '1px solid rgba(99, 102, 241, 0.35)',
        borderRadius: 'var(--radius-md)',
        padding: 24,
        marginBottom: 24,
        boxShadow: '0 8px 24px rgba(0,0,0,0.3)'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 14 }}>
          <div style={{
            width: 42,
            height: 42,
            borderRadius: 'var(--radius-sm)',
            background: 'var(--primary)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            boxShadow: '0 0 16px var(--primary-glow)'
          }}>
            <Smartphone size={24} />
          </div>
          <div>
            <h3 style={{ fontSize: '1.2rem', fontWeight: 800 }}>Como Rodar e Instalar no Celular</h3>
            <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>Progressive Web App (PWA) de Alto Desempenho</span>
          </div>
        </div>

        <ol style={{ paddingLeft: 20, fontSize: '0.9rem', color: 'var(--text-main)', display: 'flex', flexDirection: 'column', gap: 10, lineHeight: 1.5 }}>
          <li>
            <strong>Mesma rede Wi-Fi:</strong> Certifique-se de que seu celular e seu computador estejam conectados na mesma rede.
          </li>
          <li>
            <strong>Acesse o IP do seu computador:</strong> No navegador do celular (Chrome ou Safari), abra:
            <code style={{
              display: 'block',
              margin: '6px 0',
              padding: '8px 12px',
              background: '#0b0f19',
              borderRadius: 8,
              border: '1px solid var(--border-subtle)',
              color: 'var(--accent-amber)',
              fontFamily: 'monospace'
            }}>
              http://&lt;IP-DO-SEU-PC&gt;:5173
            </code>
          </li>
          <li>
            <strong>Instale como App (PWA):</strong>
            <ul style={{ paddingLeft: 18, marginTop: 4, color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              <li><strong>No Android (Chrome):</strong> Toque no menu (3 pontinhos) &rarr; <strong>"Adicionar à tela inicial"</strong> ou <strong>"Instalar aplicativo"</strong>.</li>
              <li><strong>No iPhone (Safari):</strong> Toque no botão de Compartilhar &rarr; <strong>"Adicionar à Tela de Início"</strong>.</li>
            </ul>
          </li>
          <li>
            O aplicativo abrirá em <strong>tela cheia</strong>, com ícone próprio e navegação instantânea!
          </li>
        </ol>
      </div>

      {/* Authentication Card */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border-subtle)',
        borderRadius: 'var(--radius-md)',
        padding: 24,
        marginBottom: 24
      }}>
        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <div style={{
              width: 40,
              height: 40,
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(99, 102, 241, 0.12)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--primary)'
            }}>
              <Server size={22} />
            </div>
            <div>
              <h3 style={{ fontSize: '1.15rem', fontWeight: 800 }}>Autenticação na API (JWT)</h3>
              <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                {apiService.isAuthenticated() ? '🟢 Conectado e Autenticado com Sucesso' : '🟡 Modo Offline / Não Autenticado'}
              </span>
            </div>
          </div>

          {apiService.isAuthenticated() && (
            <button
              type="button"
              onClick={() => {
                apiService.logout();
                testConnection();
              }}
              style={{
                background: 'rgba(239, 68, 68, 0.1)',
                border: '1px solid rgba(239, 68, 68, 0.3)',
                color: 'var(--danger)',
                padding: '6px 12px',
                borderRadius: 6,
                fontSize: '0.8rem',
                cursor: 'pointer',
                fontWeight: 600
              }}
            >
              Desconectar
            </button>
          )}
        </div>

        {apiService.isAuthenticated() ? (
          <div>
            <div style={{
              padding: '14px 16px',
              borderRadius: 'var(--radius-sm)',
              background: 'rgba(16, 185, 129, 0.08)',
              border: '1px solid rgba(16, 185, 129, 0.25)',
              marginBottom: 16,
              fontSize: '0.88rem'
            }}>
              <p style={{ margin: 0, color: 'var(--text-main)', lineHeight: 1.5 }}>
                ✅ <strong>O FrontEnd está autenticado e acessando as APIs diretamente!</strong> As chamadas para <code>/api/hq</code>, <code>/api/desejo</code> e <code>/api/editora</code> são enviadas com o Bearer Token e consultam o banco SQL Server em tempo real.
              </p>
            </div>

            <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
              <button
                type="button"
                className="btn-primary"
                onClick={async () => {
                  setChecking(true);
                  try {
                    const hqs = await apiService.getHQs();
                    const editoras = await apiService.getEditoras();
                    alert(`Sincronização concluída!\n\n${hqs.length} HQs carregadas da base SQL Server.\n${editoras.length} Editoras carregadas.`);
                    window.location.href = '/';
                  } catch (err: any) {
                    alert('Erro na sincronização: ' + err.message);
                  } finally {
                    setChecking(false);
                  }
                }}
                disabled={checking}
              >
                <RefreshCw size={16} className={checking ? 'animate-spin' : ''} />
                <span>Sincronizar Banco Agora (Carregar HQs)</span>
              </button>
            </div>
          </div>
        ) : (
          <div>
            <p style={{ fontSize: '0.86rem', color: 'var(--text-muted)', marginBottom: 14 }}>
              Como os endpoints da API possuem o atributo <code>[Authorize]</code>, é necessário efetuar login para que o FrontEnd obtenha o token JWT e acesse os 3.300+ quadrinhos cadastrados no SQL Server.
            </p>

            <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap' }}>
              <button
                type="button"
                className="btn-primary"
                onClick={async () => {
                  setChecking(true);
                  const res = await apiService.login('bestlinux@gmail.com', '200314Amor@');
                  setChecking(false);
                  if (res.success) {
                    alert('Login efetuado com sucesso na API .NET 10!\nToken JWT gerado.');
                    testConnection();
                  } else {
                    alert('Falha no login: ' + res.message);
                  }
                }}
                disabled={checking}
              >
                {checking ? 'Conectando...' : 'Entrar com Conta Padrão (bestlinux@gmail.com)'}
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Backend API Connection Card */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border-subtle)',
        borderRadius: 'var(--radius-md)',
        padding: 24,
        marginBottom: 24
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
          <div style={{
            width: 40,
            height: 40,
            borderRadius: 'var(--radius-sm)',
            background: 'rgba(255, 255, 255, 0.06)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--accent-amber)'
          }}>
            <Server size={22} />
          </div>
          <div>
            <h3 style={{ fontSize: '1.15rem', fontWeight: 800 }}>Status do Servidor (.NET 10)</h3>
            <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)' }}>Comunicação HTTP / Proxy Vite</span>
          </div>
        </div>

        {/* Status Indicator */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '12px 16px',
          borderRadius: 'var(--radius-sm)',
          background: pingStatus.success ? 'rgba(16, 185, 129, 0.1)' : 'rgba(239, 68, 68, 0.1)',
          border: `1px solid ${pingStatus.success ? 'rgba(16, 185, 129, 0.3)' : 'rgba(239, 68, 68, 0.3)'}`,
          marginBottom: 18
        }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            {pingStatus.success ? (
              <CheckCircle size={20} color="var(--accent-emerald)" />
            ) : (
              <AlertCircle size={20} color="var(--danger)" />
            )}
            <span style={{ fontSize: '0.88rem', fontWeight: 600 }}>
              {checking ? 'Testando conexão...' : pingStatus.message || 'Clique em testar conexão'}
            </span>
          </div>

          <button
            type="button"
            onClick={testConnection}
            disabled={checking}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--primary)',
              cursor: 'pointer',
              fontWeight: 700,
              fontSize: '0.82rem',
              display: 'flex',
              alignItems: 'center',
              gap: 4
            }}
          >
            <RefreshCw size={14} className={checking ? 'animate-spin' : ''} />
            Testar
          </button>
        </div>

        {/* Change URL form */}
        <form onSubmit={handleSaveUrl} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
          <div>
            <label style={{ display: 'block', fontSize: '0.8rem', fontWeight: 700, marginBottom: 6, color: 'var(--text-muted)' }}>
              ENDEREÇO DA API
            </label>
            <input
              type="text"
              value={apiUrl}
              onChange={(e) => setApiUrl(e.target.value)}
              placeholder="/api ou https://localhost:9001/api"
              style={{
                width: '100%',
                padding: '11px 14px',
                background: 'var(--bg-surface)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 'var(--radius-sm)',
                color: '#fff',
                fontSize: '0.92rem'
              }}
            />
            <span style={{ fontSize: '0.78rem', color: 'var(--text-dim)', marginTop: 4, display: 'block' }}>
              Padrão em desenvolvimento: <code>/api</code> (redirecionado automaticamente pelo proxy do Vite para <code>https://localhost:9001</code>).
            </span>
          </div>

          <button type="submit" className="btn-secondary" style={{ alignSelf: 'flex-start' }}>
            Salvar Endereço
          </button>
        </form>
      </div>

      {/* Cache & Maintenance */}
      <div style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--border-subtle)',
        borderRadius: 'var(--radius-md)',
        padding: 24,
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: 16
      }}>
        <div>
          <h4 style={{ fontWeight: 700, fontSize: '1rem' }}>Cache Offline & Dados Locais</h4>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.82rem', marginTop: 2 }}>
            Restaure os dados de demonstração ou limpe o cache do dispositivo.
          </p>
        </div>

        <button
          type="button"
          onClick={handleResetData}
          style={{
            background: 'rgba(239, 68, 68, 0.12)',
            color: 'var(--danger)',
            border: '1px solid rgba(239, 68, 68, 0.3)',
            borderRadius: 'var(--radius-sm)',
            padding: '8px 16px',
            fontSize: '0.82rem',
            fontWeight: 700,
            cursor: 'pointer'
          }}
        >
          Resetar Dados
        </button>
      </div>
    </div>
  );
};
