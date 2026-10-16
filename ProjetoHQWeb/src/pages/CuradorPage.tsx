import React, { useState, useEffect, useRef } from 'react';
import { 
  Sparkles, Compass, BrainCircuit, Trophy, Bot, Zap, Check, CheckCircle2, 
  Plus, Layers, AlertTriangle, Heart, ListOrdered, 
  Flame, ArrowRight, Loader2, RefreshCw, Send, CheckSquare, Square,
  ShieldCheck, Activity, BarChart3, DollarSign, Cpu, Clock, 
  Trash2, TrendingUp, History
} from 'lucide-react';
import { apiService } from '../services/api';
import { 
  OrdemLeituraResponse, DnaColecaoResponse, RecapNarrativoResponse, 
  QuizAcervoResponse, ChatMessage, ComandoCrudPreview, 
  ImportacaoLotePreview, HQ, ResumoTelemetriaIa, LogUsoIAItem
} from '../types';

interface CuradorPageProps {
  defaultTab?: 'ordem' | 'dna' | 'recap' | 'quiz' | 'chat' | 'crud' | 'lote' | 'telemetria';
}

export const CuradorPage: React.FC<CuradorPageProps> = ({ defaultTab = 'ordem' }) => {
  const [activeTab, setActiveTab] = useState<'ordem' | 'dna' | 'recap' | 'quiz' | 'chat' | 'crud' | 'lote' | 'telemetria'>(defaultTab);
  const [userHqs, setUserHqs] = useState<HQ[]>([]);
  const [notification, setNotification] = useState<{ type: 'success' | 'error' | 'info'; message: string } | null>(null);

  const showNotification = (type: 'success' | 'error' | 'info', message: string) => {
    setNotification({ type, message });
    setTimeout(() => setNotification(null), 4000);
  };

  // Carrega HQs do usuário em cache local para seleção rápida
  useEffect(() => {
    apiService.getHQs().then(data => setUserHqs(data)).catch(() => {});
  }, []);

  // ==========================================
  // 1. GUIA CRONOLÓGICO & ORDEM DE SAGAS
  // ==========================================
  const [ordemQuery, setOrdemQuery] = useState('');
  const [ordemUniverso, setOrdemUniverso] = useState('Geral');
  const [isLoadingOrdem, setIsLoadingOrdem] = useState(false);
  const [ordemResult, setOrdemResult] = useState<OrdemLeituraResponse | null>(null);
  const [addedDesejoTitles, setAddedDesejoTitles] = useState<Set<string>>(new Set());

  const handleGerarOrdem = async (customQuery?: string) => {
    const q = customQuery || ordemQuery;
    if (!q.trim()) return;
    setIsLoadingOrdem(true);
    try {
      const res = await apiService.curadorOrdemLeitura(q.trim(), ordemUniverso);
      setOrdemResult(res);
      if (customQuery) setOrdemQuery(customQuery);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao gerar cronologia.');
    } finally {
      setIsLoadingOrdem(false);
    }
  };

  const handleAddDesejoFromOrdem = async (item: { titulo: string; editoraRecomendada: string; numeroEdicao: number }) => {
    try {
      await apiService.addDesejo({
        titulo: item.titulo,
        editora: item.editoraRecomendada || 'Independente',
        numeroEdicao: item.numeroEdicao || 1,
        lido: 0
      });
      setAddedDesejoTitles(prev => new Set(prev).add(`${item.titulo}_${item.numeroEdicao}`));
      showNotification('success', `"${item.titulo}" adicionado à Lista de Desejos!`);
    } catch (err: any) {
      showNotification('error', 'Erro ao adicionar aos desejos: ' + err.message);
    }
  };

  // ==========================================
  // 2. DETETIVE DE COLEÇÃO & DNA DO ACERVO
  // ==========================================
  const [dnaResult, setDnaResult] = useState<DnaColecaoResponse | null>(null);
  const [isLoadingDna, setIsLoadingDna] = useState(false);

  const handleCarregarDna = async () => {
    setIsLoadingDna(true);
    try {
      const res = await apiService.curadorDnaGaps();
      setDnaResult(res);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao analisar DNA da coleção.');
    } finally {
      setIsLoadingDna(false);
    }
  };

  useEffect(() => {
    if (activeTab === 'dna' && !dnaResult) {
      handleCarregarDna();
    }
  }, [activeTab]);

  // ==========================================
  // 3. STORYTELLER / RECAP NARRATIVO
  // ==========================================
  const [recapTitulo, setRecapTitulo] = useState('');
  const [recapEdicao, setRecapEdicao] = useState('1');
  const [recapEditora, setRecapEditora] = useState('');
  const [isLoadingRecap, setIsLoadingRecap] = useState(false);
  const [recapResult, setRecapResult] = useState<RecapNarrativoResponse | null>(null);

  const handleGerarRecap = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!recapTitulo.trim()) return;
    setIsLoadingRecap(true);
    try {
      const res = await apiService.curadorRecap(
        recapTitulo.trim(),
        parseInt(recapEdicao, 10) || 1,
        recapEditora.trim() || undefined
      );
      setRecapResult(res);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao gerar recap narrativo.');
    } finally {
      setIsLoadingRecap(false);
    }
  };

  const handleSelectHqForRecap = (hq: HQ) => {
    setRecapTitulo(hq.titulo);
    setRecapEdicao(String(hq.numeroEdicao || 1));
    setRecapEditora(hq.editora || '');
  };

  // ==========================================
  // 4. TRIVIA & QUIZ DO ACERVO
  // ==========================================
  const [quizResult, setQuizResult] = useState<QuizAcervoResponse | null>(null);
  const [isLoadingQuiz, setIsLoadingQuiz] = useState(false);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [selectedAnswer, setSelectedAnswer] = useState<number | null>(null);
  const [score, setScore] = useState(0);
  const [quizFinished, setQuizFinished] = useState(false);

  const handleGerarQuiz = async () => {
    setIsLoadingQuiz(true);
    setCurrentQuestionIndex(0);
    setSelectedAnswer(null);
    setScore(0);
    setQuizFinished(false);
    try {
      const res = await apiService.curadorQuiz(5);
      setQuizResult(res);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao gerar quiz.');
    } finally {
      setIsLoadingQuiz(false);
    }
  };

  useEffect(() => {
    if (activeTab === 'quiz' && !quizResult) {
      handleGerarQuiz();
    }
  }, [activeTab]);

  const handleAnswerClick = (optionIndex: number) => {
    if (selectedAnswer !== null || !quizResult) return;
    setSelectedAnswer(optionIndex);
    const curr = quizResult.perguntas[currentQuestionIndex];
    if (optionIndex === curr.respostaCorretaIndex) {
      setScore(prev => prev + 1);
    }
  };

  const handleNextQuestion = () => {
    if (!quizResult) return;
    if (currentQuestionIndex + 1 < quizResult.perguntas.length) {
      setCurrentQuestionIndex(prev => prev + 1);
      setSelectedAnswer(null);
    } else {
      setQuizFinished(true);
    }
  };

  // ==========================================
  // 5. CURADOR VIRTUAL (CHATBOT COM RAG)
  // ==========================================
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([
    {
      remetente: 'model',
      texto: 'Olá! Sou o **Curador Virtual** do seu acervo. Tenho acesso completo a todas as HQs da sua estante, histórico de leitura e preferências. Como posso te orientar hoje?'
    }
  ]);
  const [chatInput, setChatInput] = useState('');
  const [isSendingChat, setIsSendingChat] = useState(false);
  const chatEndRef = useRef<HTMLDivElement>(null);

  const handleSendChat = async (messageToSend?: string) => {
    const text = messageToSend || chatInput;
    if (!text.trim() || isSendingChat) return;

    const newHistory: ChatMessage[] = [...chatMessages, { remetente: 'user', texto: text }];
    setChatMessages(newHistory);
    setChatInput('');
    setIsSendingChat(true);

    try {
      const res = await apiService.curadorChatbot(text, newHistory);
      setChatMessages(prev => [...prev, { 
        remetente: 'model', 
        texto: res.resposta,
        origemMotor: res.origemMotor
      }]);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro no chat.');
    } finally {
      setIsSendingChat(false);
      setTimeout(() => chatEndRef.current?.scrollIntoView({ behavior: 'smooth' }), 100);
    }
  };

  // ==========================================
  // 6. COMANDOS CRUD EM LINGUAGEM NATURAL
  // ==========================================
  const [crudInput, setCrudInput] = useState('');
  const [isLoadingCrudPreview, setIsLoadingCrudPreview] = useState(false);
  const [crudPreview, setCrudPreview] = useState<ComandoCrudPreview | null>(null);
  const [isExecutingCrud, setIsExecutingCrud] = useState(false);

  const handleGerarPreviewCrud = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!crudInput.trim()) return;
    setIsLoadingCrudPreview(true);
    setCrudPreview(null);
    try {
      const res = await apiService.curadorPreviewComando(crudInput.trim());
      setCrudPreview(res);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao interpretar comando.');
    } finally {
      setIsLoadingCrudPreview(false);
    }
  };

  const handleExecutarCrud = async () => {
    if (!crudPreview) return;
    setIsExecutingCrud(true);
    try {
      const res = await apiService.curadorExecutarComando({
        acao: crudPreview.acao,
        hqIdAlvo: crudPreview.hqIdAlvo,
        titulo: crudPreview.tituloAlvo,
        editora: crudPreview.editoraAlvo,
        numeroEdicao: crudPreview.edicaoAlvo || 1
      });
      if (res.sucesso) {
        showNotification('success', res.mensagem);
        setCrudPreview(null);
        setCrudInput('');
        // Recarrega acervo
        apiService.getHQs().then(data => setUserHqs(data)).catch(() => {});
      } else {
        showNotification('error', res.mensagem);
      }
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao executar ação.');
    } finally {
      setIsExecutingCrud(false);
    }
  };

  // ==========================================
  // 7. IMPORTAÇÃO EM LOTE ASSISTIDA
  // ==========================================
  const [loteTexto, setLoteTexto] = useState('');
  const [isLoadingLote, setIsLoadingLote] = useState(false);
  const [lotePreview, setLotePreview] = useState<ImportacaoLotePreview | null>(null);
  const [loteDestino, setLoteDestino] = useState<'colecao' | 'desejos'>('colecao');
  const [isSavingLote, setIsSavingLote] = useState(false);

  const handleGerarLote = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!loteTexto.trim()) return;
    setIsLoadingLote(true);
    try {
      const res = await apiService.curadorPreviewLote(loteTexto.trim());
      setLotePreview(res);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao interpretar lote.');
    } finally {
      setIsLoadingLote(false);
    }
  };

  const handleToggleItemLote = (idTemp: number) => {
    if (!lotePreview) return;
    setLotePreview({
      ...lotePreview,
      itens: lotePreview.itens.map(item => 
        item.idTemp === idTemp ? { ...item, selecionado: !item.selecionado } : item
      )
    });
  };

  const handleSalvarLote = async () => {
    if (!lotePreview) return;
    const selecionados = lotePreview.itens.filter(i => i.selecionado);
    if (selecionados.length === 0) {
      showNotification('info', 'Selecione pelo menos um quadrinho.');
      return;
    }
    setIsSavingLote(true);
    try {
      const res = await apiService.curadorSalvarLote(selecionados, loteDestino);
      showNotification('success', res.mensagem);
      setLotePreview(null);
      setLoteTexto('');
      // Atualiza acervo
      apiService.getHQs().then(data => setUserHqs(data)).catch(() => {});
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao salvar quadrinhos.');
    } finally {
      setIsSavingLote(false);
    }
  };

  // ==========================================
  // 8. RELATÓRIO DE IA & OBSERVABILIDADE (OPENTELEMETRY)
  // ==========================================
  const [telemetriaResumo, setTelemetriaResumo] = useState<ResumoTelemetriaIa | null>(null);
  const [telemetriaHistorico, setTelemetriaHistorico] = useState<LogUsoIAItem[]>([]);
  const [isLoadingTelemetria, setIsLoadingTelemetria] = useState(false);
  const [filtroMotor, setFiltroMotor] = useState<'todos' | 'gemini' | 'jev'>('todos');

  const carregarTelemetria = async () => {
    setIsLoadingTelemetria(true);
    try {
      const [resumo, historico] = await Promise.all([
        apiService.curadorTelemetriaResumo(),
        apiService.curadorTelemetriaHistorico(50)
      ]);
      setTelemetriaResumo(resumo);
      setTelemetriaHistorico(historico);
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao carregar telemetria de IA.');
    } finally {
      setIsLoadingTelemetria(false);
    }
  };

  const handleLimparTelemetria = async () => {
    if (!window.confirm('Deseja realmente limpar o histórico de logs de telemetria de IA?')) return;
    setIsLoadingTelemetria(true);
    try {
      await apiService.curadorTelemetriaLimpar();
      showNotification('success', 'Histórico de telemetria reiniciado com sucesso!');
      await carregarTelemetria();
    } catch (err: any) {
      showNotification('error', err.message || 'Erro ao reiniciar telemetria.');
    } finally {
      setIsLoadingTelemetria(false);
    }
  };

  useEffect(() => {
    if (activeTab === 'telemetria') {
      carregarTelemetria();
    }
  }, [activeTab]);

  return (
    <div style={{ maxWidth: 1300, margin: '0 auto', padding: '24px 16px 80px' }}>
      {/* Toast Notification */}
      {notification && (
        <div style={{
          position: 'fixed',
          top: 24,
          right: 24,
          zIndex: 9999,
          padding: '12px 20px',
          borderRadius: 8,
          background: notification.type === 'success' ? '#10b981' : notification.type === 'error' ? '#ef4444' : '#3b82f6',
          color: '#fff',
          fontWeight: 600,
          boxShadow: '0 8px 24px rgba(0,0,0,0.4)',
          display: 'flex',
          alignItems: 'center',
          gap: 10
        }}>
          {notification.type === 'success' && <CheckCircle2 size={18} />}
          {notification.type === 'error' && <AlertTriangle size={18} />}
          {notification.message}
        </div>
      )}

      {/* Hero Header */}
      <div style={{
        background: 'linear-gradient(135deg, rgba(30, 27, 75, 0.8) 0%, rgba(15, 23, 42, 0.9) 100%)',
        border: '1px solid rgba(168, 85, 247, 0.25)',
        borderRadius: 16,
        padding: '28px 32px',
        marginBottom: 24,
        position: 'relative',
        overflow: 'hidden',
        boxShadow: '0 12px 36px rgba(0,0,0,0.3)'
      }}>
        <div style={{
          position: 'absolute',
          top: -40,
          right: -40,
          width: 200,
          height: 200,
          borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(168, 85, 247, 0.25) 0%, rgba(0,0,0,0) 70%)',
          pointerEvents: 'none'
        }} />

        <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 16 }}>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
              <span style={{
                background: 'linear-gradient(90deg, #a855f7, #06b6d4)',
                padding: '4px 10px',
                borderRadius: 20,
                fontSize: '0.72rem',
                fontWeight: 800,
                color: '#fff',
                letterSpacing: '0.08em',
                textTransform: 'uppercase',
                display: 'inline-flex',
                alignItems: 'center',
                gap: 6
              }}>
                <Sparkles size={12} /> Hub de Inteligência Artificial
              </span>
              <span style={{
                fontSize: '0.78rem',
                color: 'var(--text-muted)',
                display: 'inline-flex',
                alignItems: 'center',
                gap: 6
              }}>
                <ShieldCheck size={14} color="#10b981" /> Gemini 3.8 Flash Conectado
              </span>
            </div>

            <h1 style={{ fontSize: '2.1rem', fontWeight: 800, color: '#fff', margin: 0, letterSpacing: '-0.02em' }}>
              Curador de HQs
            </h1>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem', margin: '6px 0 0', maxWidth: 640 }}>
              Sete agentes autônomos para orquestrar cronologias de sagas, decodificar o DNA do seu acervo, contar antecedentes narrativos e automatizar sua coleção.
            </p>
          </div>

          <div style={{
            display: 'flex',
            alignItems: 'center',
            gap: 16,
            background: 'rgba(0,0,0,0.35)',
            border: '1px solid rgba(255,255,255,0.08)',
            padding: '12px 18px',
            borderRadius: 12
          }}>
            <div>
              <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>ACERVO VINCULADO</div>
              <div style={{ fontSize: '1.25rem', fontWeight: 800, color: 'var(--accent-cyan)' }}>
                {userHqs.length} HQs
              </div>
            </div>
            <div style={{ width: 1, height: 32, background: 'rgba(255,255,255,0.1)' }} />
            <div>
              <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>TAXA DE LEITURA</div>
              <div style={{ fontSize: '1.25rem', fontWeight: 800, color: '#10b981' }}>
                {userHqs.length > 0 ? Math.round((userHqs.filter(h => h.lido === 1).length / userHqs.length) * 100) : 0}%
              </div>
            </div>
            <div style={{ width: 1, height: 32, background: 'rgba(255,255,255,0.1)' }} />
            <button
              type="button"
              onClick={() => setActiveTab('telemetria')}
              style={{
                background: activeTab === 'telemetria' ? 'linear-gradient(135deg, rgba(168, 85, 247, 0.4) 0%, rgba(6, 182, 212, 0.3) 100%)' : 'rgba(255,255,255,0.06)',
                border: '1px solid rgba(168, 85, 247, 0.4)',
                borderRadius: 8,
                padding: '6px 12px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                gap: 6,
                color: '#fff',
                fontSize: '0.78rem',
                fontWeight: 700,
                transition: 'all 0.2s'
              }}
            >
              <Activity size={14} color="var(--accent-cyan)" />
              <span>Relatório OTel</span>
            </button>
          </div>
        </div>
      </div>

      {/* Navigation Tabs (8 Specialized Agents & Observability) */}
      <div style={{
        display: 'flex',
        gap: 8,
        overflowX: 'auto',
        paddingBottom: 10,
        marginBottom: 24,
        borderBottom: '1px solid var(--border-subtle)'
      }}>
        {[
          { id: 'ordem', label: '1. Ordem de Sagas', icon: ListOrdered, desc: 'Cronologia e cruzamento' },
          { id: 'dna', label: '2. DNA & Gaps', icon: BrainCircuit, desc: 'Arquétipo e lacunas' },
          { id: 'recap', label: '3. Storyteller', icon: Flame, desc: 'Previously On...' },
          { id: 'quiz', label: '4. Trivia do Acervo', icon: Trophy, desc: 'Quiz da sua estante' },
          { id: 'chat', label: '5. Curador Virtual', icon: Bot, desc: 'Chatbot com RAG' },
          { id: 'crud', label: '6. Comandos Naturais', icon: Zap, desc: 'Ações via texto' },
          { id: 'lote', label: '7. Importação em Lote', icon: Layers, desc: 'Texto para acervo' },
          { id: 'telemetria', label: '8. Relatório de IA (OTel)', icon: Activity, desc: 'Observabilidade & Custos' }
        ].map(tab => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              style={{
                display: 'flex',
                alignItems: 'center',
                gap: 8,
                padding: '10px 16px',
                borderRadius: 10,
                background: isActive ? 'linear-gradient(135deg, rgba(168, 85, 247, 0.25) 0%, rgba(6, 182, 212, 0.15) 100%)' : 'var(--bg-card)',
                border: isActive ? '1px solid var(--accent-purple)' : '1px solid var(--border-subtle)',
                color: isActive ? '#fff' : 'var(--text-muted)',
                fontWeight: isActive ? 700 : 500,
                fontSize: '0.86rem',
                cursor: 'pointer',
                whiteSpace: 'nowrap',
                transition: 'all 0.2s ease',
                boxShadow: isActive ? '0 4px 14px rgba(168, 85, 247, 0.2)' : 'none'
              }}
            >
              <Icon size={16} color={isActive ? 'var(--accent-cyan)' : 'currentColor'} />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* TAB 1: GUIA CRONOLÓGICO & ORDEM DE SAGAS */}
      {activeTab === 'ordem' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 700, margin: '0 0 8px', color: '#fff' }}>
              Guia Cronológico de Leitura & Ordem de Sagas
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', margin: '0 0 18px' }}>
              Constrói a cronologia ideal para sagas, personagens ou universos inteiros e cruza diretamente com seu acervo para destacar o que você já possui e o que falta na estante.
            </p>

            {/* Quick Sugestions Chips */}
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap', marginBottom: 16 }}>
              <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 600 }}>Sugestões rápidas:</span>
              {[
                'Guerra Civil (Marvel)',
                'Crise nas Infinitas Terras',
                'Sandman (Vertigo)',
                'Batman: A Queda do Morcego',
                'Saga do Clone',
                'Flashpoint',
                'A Noite Mais Densa'
              ].map(chip => (
                <button
                  key={chip}
                  type="button"
                  onClick={() => handleGerarOrdem(chip)}
                  style={{
                    background: 'rgba(255,255,255,0.05)',
                    border: '1px solid var(--border-subtle)',
                    padding: '4px 10px',
                    borderRadius: 16,
                    fontSize: '0.78rem',
                    color: 'var(--text-primary)',
                    cursor: 'pointer'
                  }}
                >
                  {chip}
                </button>
              ))}
            </div>

            <form onSubmit={(e) => { e.preventDefault(); handleGerarOrdem(); }} style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
              <input
                type="text"
                placeholder="Ex: Guerra Civil, Reino do Amanhã, Guerras Secretas..."
                value={ordemQuery}
                onChange={(e) => setOrdemQuery(e.target.value)}
                style={{
                  flex: 3,
                  minWidth: 260,
                  padding: '12px 16px',
                  background: 'var(--bg-card-hover)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 8,
                  color: '#fff',
                  fontSize: '0.95rem'
                }}
              />
              <select
                value={ordemUniverso}
                onChange={(e) => setOrdemUniverso(e.target.value)}
                style={{
                  flex: 1,
                  minWidth: 140,
                  padding: '12px 14px',
                  background: 'var(--bg-card-hover)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 8,
                  color: '#fff',
                  fontSize: '0.9rem'
                }}
              >
                <option value="Geral">Universo: Geral</option>
                <option value="Marvel">Marvel Comics</option>
                <option value="DC">DC Comics</option>
                <option value="Vertigo">Vertigo / Black Label</option>
                <option value="Mangá">Mangás</option>
              </select>
              <button
                type="submit"
                className="btn-primary"
                disabled={isLoadingOrdem || !ordemQuery.trim()}
                style={{ padding: '12px 24px', display: 'flex', alignItems: 'center', gap: 8 }}
              >
                {isLoadingOrdem ? <Loader2 size={18} className="animate-spin" /> : <Compass size={18} />}
                Gerar Guia Cronológico
              </button>
            </form>
          </div>

          {/* Results Presentation */}
          {ordemResult && (
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 14,
              padding: 24
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 14, marginBottom: 20 }}>
                <div>
                  <span style={{ fontSize: '0.78rem', color: 'var(--accent-purple)', fontWeight: 700, textTransform: 'uppercase' }}>
                    {ordemResult.universo}
                  </span>
                  <h3 style={{ fontSize: '1.5rem', fontWeight: 800, color: '#fff', margin: '4px 0 8px' }}>
                    {ordemResult.saga}
                  </h3>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', maxWidth: 800, margin: 0 }}>
                    {ordemResult.descricaoGeral}
                  </p>
                </div>

                <div style={{ display: 'flex', gap: 12 }}>
                  <div style={{
                    background: 'rgba(16, 185, 129, 0.1)',
                    border: '1px solid rgba(16, 185, 129, 0.3)',
                    padding: '8px 14px',
                    borderRadius: 8,
                    textAlign: 'center'
                  }}>
                    <div style={{ fontSize: '1.15rem', fontWeight: 800, color: '#10b981' }}>{ordemResult.totalItensNaEstante}</div>
                    <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>NA SUA ESTANTE</div>
                  </div>
                  <div style={{
                    background: 'rgba(239, 68, 68, 0.1)',
                    border: '1px solid rgba(239, 68, 68, 0.3)',
                    padding: '8px 14px',
                    borderRadius: 8,
                    textAlign: 'center'
                  }}>
                    <div style={{ fontSize: '1.15rem', fontWeight: 800, color: '#ef4444' }}>{ordemResult.totalItensAusentes}</div>
                    <div style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>VOLUMES AUSENTES</div>
                  </div>
                </div>
              </div>

              {/* Timeline Cards */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
                {ordemResult.itens.map((item, idx) => {
                  const key = `${item.titulo}_${item.numeroEdicao}`;
                  const isAdded = addedDesejoTitles.has(key);

                  return (
                    <div
                      key={idx}
                      style={{
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        padding: '16px 20px',
                        background: item.naEstante ? 'rgba(16, 185, 129, 0.04)' : 'rgba(255, 255, 255, 0.02)',
                        border: item.naEstante ? '1px solid rgba(16, 185, 129, 0.25)' : '1px solid var(--border-subtle)',
                        borderRadius: 10,
                        gap: 16,
                        flexWrap: 'wrap'
                      }}
                    >
                      <div style={{ display: 'flex', alignItems: 'center', gap: 14, flex: 1, minWidth: 260 }}>
                        <div style={{
                          width: 32,
                          height: 32,
                          borderRadius: '50%',
                          background: item.naEstante ? '#10b981' : 'var(--bg-card-hover)',
                          color: item.naEstante ? '#fff' : 'var(--text-muted)',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontWeight: 800,
                          fontSize: '0.85rem'
                        }}>
                          {item.ordem}
                        </div>

                        <div>
                          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                            <span style={{ fontSize: '1rem', fontWeight: 700, color: '#fff' }}>
                              {item.titulo}
                            </span>
                            {item.numeroEdicao > 0 && (
                              <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                                #{item.numeroEdicao}
                              </span>
                            )}
                            <span style={{
                              fontSize: '0.7rem',
                              padding: '2px 8px',
                              borderRadius: 4,
                              background: item.importancia === 'Essencial' ? 'rgba(239, 68, 68, 0.15)' : 'rgba(255,255,255,0.08)',
                              color: item.importancia === 'Essencial' ? '#f87171' : 'var(--text-muted)',
                              fontWeight: 700
                            }}>
                              {item.importancia}
                            </span>
                          </div>

                          <p style={{ margin: '4px 0 0', fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                            {item.sinopseCurta} • <span style={{ color: 'var(--accent-purple)' }}>{item.editoraRecomendada}</span>
                          </p>
                        </div>
                      </div>

                      {/* Status / Action Button */}
                      <div>
                        {item.naEstante ? (
                          <span style={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            gap: 6,
                            padding: '6px 14px',
                            borderRadius: 6,
                            background: 'rgba(16, 185, 129, 0.15)',
                            color: '#10b981',
                            fontWeight: 700,
                            fontSize: '0.82rem'
                          }}>
                            <Check size={16} /> Na sua estante {item.lido ? '(Lido)' : '(Não lido)'}
                          </span>
                        ) : (
                          <button
                            type="button"
                            onClick={() => handleAddDesejoFromOrdem(item)}
                            disabled={isAdded}
                            style={{
                              display: 'inline-flex',
                              alignItems: 'center',
                              gap: 6,
                              padding: '6px 14px',
                              borderRadius: 6,
                              background: isAdded ? 'rgba(255,255,255,0.08)' : 'rgba(245, 158, 11, 0.15)',
                              border: isAdded ? '1px solid transparent' : '1px solid rgba(245, 158, 11, 0.4)',
                              color: isAdded ? 'var(--text-muted)' : '#f59e0b',
                              fontWeight: 700,
                              fontSize: '0.82rem',
                              cursor: isAdded ? 'default' : 'pointer'
                            }}
                          >
                            <Heart size={14} fill={isAdded ? 'currentColor' : 'none'} />
                            {isAdded ? 'Adicionado aos Desejos' : '+ Lista de Desejos'}
                          </button>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          )}
        </div>
      )}

      {/* TAB 2: DETETIVE DE COLEÇÃO & DNA DO ACERVO */}
      {activeTab === 'dna' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          {isLoadingDna && (
            <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-muted)' }}>
              <Loader2 size={36} className="animate-spin" style={{ margin: '0 auto 12px', color: 'var(--accent-purple)' }} />
              <p style={{ fontSize: '1rem', fontWeight: 600 }}>Decodificando o DNA do seu acervo e farejando lacunas...</p>
            </div>
          )}

          {!isLoadingDna && dnaResult && (
            <>
              {/* Archetype Card */}
              <div style={{
                background: 'linear-gradient(135deg, rgba(88, 28, 135, 0.4) 0%, rgba(15, 23, 42, 0.8) 100%)',
                border: '1px solid rgba(168, 85, 247, 0.3)',
                borderRadius: 14,
                padding: 28,
                position: 'relative'
              }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 8 }}>
                  <BrainCircuit size={20} color="var(--accent-cyan)" />
                  <span style={{ fontSize: '0.78rem', fontWeight: 700, color: 'var(--accent-cyan)', textTransform: 'uppercase' }}>
                    Arquétipo Psicológico do Colecionador
                  </span>
                </div>

                <h2 style={{ fontSize: '1.9rem', fontWeight: 800, color: '#fff', margin: '4px 0 6px' }}>
                  {dnaResult.arquetipo}
                </h2>
                <div style={{ fontSize: '1rem', fontStyle: 'italic', color: '#c084fc', marginBottom: 14 }}>
                  "{dnaResult.subtituloArquetipo}"
                </div>

                <p style={{ color: 'var(--text-muted)', fontSize: '0.92rem', lineHeight: 1.6, margin: 0, maxWidth: 900 }}>
                  {dnaResult.descricaoArquetipo}
                </p>

                {/* Highlights */}
                {dnaResult.pontosFortesColecao?.length > 0 && (
                  <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap', marginTop: 18 }}>
                    {dnaResult.pontosFortesColecao.map((ponto, i) => (
                      <span key={i} style={{
                        background: 'rgba(255,255,255,0.06)',
                        padding: '4px 12px',
                        borderRadius: 20,
                        fontSize: '0.8rem',
                        color: '#fff',
                        fontWeight: 600
                      }}>
                        ✦ {ponto}
                      </span>
                    ))}
                  </div>
                )}
              </div>

              {/* Gaps & Lacunas Sequenciais */}
              <div style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 14,
                padding: 24
              }}>
                <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 16 }}>
                  <div>
                    <h3 style={{ fontSize: '1.2rem', fontWeight: 700, color: '#fff', margin: 0, display: 'flex', alignItems: 'center', gap: 8 }}>
                      <AlertTriangle size={18} color="#f59e0b" />
                      Detecção Automática de Lacunas (Gaps)
                    </h3>
                    <p style={{ color: 'var(--text-muted)', fontSize: '0.84rem', margin: '4px 0 0' }}>
                      Identifica edições faltantes em séries que você já começou a colecionar na estante.
                    </p>
                  </div>

                  <span style={{
                    background: 'rgba(245, 158, 11, 0.15)',
                    color: '#f59e0b',
                    padding: '4px 12px',
                    borderRadius: 20,
                    fontSize: '0.8rem',
                    fontWeight: 700
                  }}>
                    {dnaResult.gapsDetectados.length} Série(s) com Gaps
                  </span>
                </div>

                {dnaResult.gapsDetectados.length === 0 ? (
                  <div style={{ padding: '24px 0', textAlign: 'center', color: 'var(--text-muted)', fontSize: '0.9rem' }}>
                    Nenhuma lacuna sequencial encontrada! Suas séries com múltiplas edições estão completas.
                  </div>
                ) : (
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(360px, 1fr))', gap: 14 }}>
                    {dnaResult.gapsDetectados.map((gap, i) => (
                      <div
                        key={i}
                        style={{
                          background: 'rgba(255,255,255,0.02)',
                          border: gap.urgencia === 'Alta' ? '1px solid rgba(239, 68, 68, 0.35)' : '1px solid var(--border-subtle)',
                          borderRadius: 10,
                          padding: 16
                        }}
                      >
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                          <h4 style={{ fontSize: '0.95rem', fontWeight: 700, color: '#fff', margin: 0 }}>
                            {gap.serie}
                          </h4>
                          <span style={{
                            fontSize: '0.7rem',
                            fontWeight: 700,
                            padding: '2px 8px',
                            borderRadius: 4,
                            background: gap.urgencia === 'Alta' ? 'rgba(239, 68, 68, 0.2)' : 'rgba(245, 158, 11, 0.2)',
                            color: gap.urgencia === 'Alta' ? '#f87171' : '#fbbf24'
                          }}>
                            {gap.urgencia} Prioridade
                          </span>
                        </div>

                        <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: 8 }}>
                          Editora: <strong style={{ color: '#fff' }}>{gap.editora}</strong>
                        </div>

                        <div style={{ fontSize: '0.82rem', color: '#f59e0b', marginBottom: 12 }}>
                          {gap.motivoGap}
                        </div>

                        {/* Botão para adicionar a primeira faltante à lista de desejos */}
                        {gap.edicoesFaltantes.length > 0 && (
                          <button
                            type="button"
                            onClick={() => handleAddDesejoFromOrdem({
                              titulo: gap.serie,
                              editoraRecomendada: gap.editora,
                              numeroEdicao: gap.edicoesFaltantes[0]
                            })}
                            style={{
                              width: '100%',
                              padding: '6px 12px',
                              borderRadius: 6,
                              background: 'rgba(255,255,255,0.06)',
                              border: '1px solid var(--border-subtle)',
                              color: '#fff',
                              fontSize: '0.78rem',
                              fontWeight: 600,
                              cursor: 'pointer',
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                              gap: 6
                            }}
                          >
                            <Plus size={14} /> Adicionar #{gap.edicoesFaltantes[0]} aos Desejos
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                )}
              </div>

              {/* Recomendações Preditivas */}
              <div style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 14,
                padding: 24
              }}>
                <h3 style={{ fontSize: '1.2rem', fontWeight: 700, color: '#fff', margin: '0 0 16px', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Sparkles size={18} color="var(--accent-purple)" />
                  Recomendações Cirúrgicas para Próximas Aquisições
                </h3>

                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))', gap: 14 }}>
                  {dnaResult.recomendacoesProximasAquisicoes?.map((rec, i) => (
                    <div
                      key={i}
                      style={{
                        background: 'rgba(255,255,255,0.02)',
                        border: '1px solid var(--border-subtle)',
                        borderRadius: 10,
                        padding: 16,
                        display: 'flex',
                        flexDirection: 'column',
                        justifyContent: 'space-between'
                      }}
                    >
                      <div>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
                          <span style={{ fontSize: '0.75rem', color: 'var(--accent-purple)', fontWeight: 700 }}>
                            {rec.editora} • #{rec.numeroEdicao}
                          </span>
                          <span style={{ fontSize: '0.75rem', color: '#10b981', fontWeight: 800 }}>
                            {rec.matchScore} Match
                          </span>
                        </div>

                        <h4 style={{ fontSize: '1rem', fontWeight: 700, color: '#fff', margin: '0 0 6px' }}>
                          {rec.titulo}
                        </h4>

                        <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', margin: '0 0 14px', lineHeight: 1.4 }}>
                          {rec.motivo}
                        </p>
                      </div>

                      <button
                        type="button"
                        onClick={() => handleAddDesejoFromOrdem({
                          titulo: rec.titulo,
                          editoraRecomendada: rec.editora,
                          numeroEdicao: rec.numeroEdicao
                        })}
                        style={{
                          padding: '6px 12px',
                          borderRadius: 6,
                          background: 'rgba(168, 85, 247, 0.15)',
                          border: '1px solid rgba(168, 85, 247, 0.4)',
                          color: '#c084fc',
                          fontSize: '0.8rem',
                          fontWeight: 700,
                          cursor: 'pointer',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          gap: 6
                        }}
                      >
                        <Heart size={14} /> Salvar nos Desejos
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </>
          )}
        </div>
      )}

      {/* TAB 3: STORYTELLER / AQUECIMENTO NARRATIVO ("PREVIOUSLY ON...") */}
      {activeTab === 'recap' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 700, margin: '0 0 6px', color: '#fff', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Flame size={20} color="#f97316" />
              Storyteller • Aquecimento Narrativo Imersivo
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', margin: '0 0 16px' }}>
              Produz uma narrativa dramática no formato "Previously on..." antes de você abrir uma nova edição, contextualizando os arcos precedentes para você mergulhar 100% ambientado.
            </p>

            {/* Quick Picker from user HQs */}
            {userHqs.length > 0 && (
              <div style={{ marginBottom: 16 }}>
                <span style={{ fontSize: '0.76rem', color: 'var(--text-muted)', fontWeight: 600, display: 'block', marginBottom: 6 }}>
                  Ou selecione uma HQ da sua estante:
                </span>
                <div style={{ display: 'flex', gap: 8, overflowX: 'auto', paddingBottom: 6 }}>
                  {userHqs.slice(0, 8).map(hq => (
                    <button
                      key={hq.id}
                      type="button"
                      onClick={() => handleSelectHqForRecap(hq)}
                      style={{
                        background: 'rgba(255,255,255,0.04)',
                        border: '1px solid var(--border-subtle)',
                        borderRadius: 6,
                        padding: '4px 10px',
                        fontSize: '0.78rem',
                        color: '#fff',
                        whiteSpace: 'nowrap',
                        cursor: 'pointer'
                      }}
                    >
                      {hq.titulo} #{hq.numeroEdicao}
                    </button>
                  ))}
                </div>
              </div>
            )}

            <form onSubmit={handleGerarRecap} style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr auto', gap: 10, alignItems: 'flex-end' }}>
              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 700, color: 'var(--text-muted)', marginBottom: 4 }}>
                  TÍTULO DA HQ *
                </label>
                <input
                  type="text"
                  required
                  placeholder="Ex: Batman: O Longo Dia das Bruxas, Sandman"
                  value={recapTitulo}
                  onChange={(e) => setRecapTitulo(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 14px',
                    background: 'var(--bg-card-hover)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 8,
                    color: '#fff',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 700, color: 'var(--text-muted)', marginBottom: 4 }}>
                  EDIÇÃO #
                </label>
                <input
                  type="number"
                  placeholder="Ex: 1"
                  value={recapEdicao}
                  onChange={(e) => setRecapEdicao(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 14px',
                    background: 'var(--bg-card-hover)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 8,
                    color: '#fff',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              <div>
                <label style={{ display: 'block', fontSize: '0.75rem', fontWeight: 700, color: 'var(--text-muted)', marginBottom: 4 }}>
                  EDITORA (OPCIONAL)
                </label>
                <input
                  type="text"
                  placeholder="Ex: Panini, Abril"
                  value={recapEditora}
                  onChange={(e) => setRecapEditora(e.target.value)}
                  style={{
                    width: '100%',
                    padding: '10px 14px',
                    background: 'var(--bg-card-hover)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 8,
                    color: '#fff',
                    fontSize: '0.9rem'
                  }}
                />
              </div>

              <button
                type="submit"
                className="btn-primary"
                disabled={isLoadingRecap || !recapTitulo.trim()}
                style={{ padding: '10px 20px', display: 'flex', alignItems: 'center', gap: 8, height: 44 }}
              >
                {isLoadingRecap ? <Loader2 size={16} className="animate-spin" /> : <Flame size={16} />}
                Gerar "Previously On..."
              </button>
            </form>
          </div>

          {/* Cinematic Recap Display */}
          {recapResult && (
            <div style={{
              background: 'linear-gradient(180deg, #18181b 0%, #09090b 100%)',
              border: '1px solid rgba(249, 115, 22, 0.3)',
              borderRadius: 14,
              padding: '32px 36px',
              boxShadow: '0 16px 48px rgba(0,0,0,0.5)',
              position: 'relative'
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <span style={{
                  color: '#f97316',
                  fontSize: '0.78rem',
                  fontWeight: 800,
                  letterSpacing: '0.12em',
                  textTransform: 'uppercase'
                }}>
                  ● PREVIOUSLY ON...
                </span>
                <span style={{
                  background: 'rgba(249, 115, 22, 0.1)',
                  border: '1px solid rgba(249, 115, 22, 0.3)',
                  padding: '4px 12px',
                  borderRadius: 20,
                  fontSize: '0.75rem',
                  color: '#fb923c',
                  fontWeight: 700
                }}>
                  {recapResult.climaOuTom}
                </span>
              </div>

              <h2 style={{ fontSize: '1.8rem', fontWeight: 800, color: '#fff', margin: '0 0 4px' }}>
                {recapResult.titulo} #{recapResult.numeroEdicao}
              </h2>
              <div style={{ fontSize: '0.92rem', color: 'var(--text-muted)', marginBottom: 20 }}>
                Arco: <strong style={{ color: '#fff' }}>{recapResult.arcoNarrativo}</strong>
              </div>

              {/* Dramatic Body */}
              <div style={{
                fontSize: '1.05rem',
                lineHeight: 1.8,
                color: '#e4e4e7',
                fontFamily: 'Georgia, serif',
                whiteSpace: 'pre-line',
                borderLeft: '3px solid #f97316',
                paddingLeft: 20,
                marginBottom: 24
              }}>
                {recapResult.previouslyOnText}
              </div>

              {/* Characters & Key events */}
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))', gap: 16, paddingTop: 16, borderTop: '1px solid rgba(255,255,255,0.08)' }}>
                <div>
                  <span style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--text-muted)', textTransform: 'uppercase', display: 'block', marginBottom: 6 }}>
                    Personagens no Centro da Trama:
                  </span>
                  <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
                    {recapResult.personagensChave?.map((p, i) => (
                      <span key={i} style={{ background: 'rgba(255,255,255,0.08)', padding: '3px 10px', borderRadius: 4, fontSize: '0.78rem', color: '#fff' }}>
                        {p}
                      </span>
                    ))}
                  </div>
                </div>

                <div>
                  <span style={{ fontSize: '0.75rem', fontWeight: 700, color: 'var(--text-muted)', textTransform: 'uppercase', display: 'block', marginBottom: 6 }}>
                    Acontecimentos Críticos Anteriores:
                  </span>
                  <ul style={{ margin: 0, paddingLeft: 18, fontSize: '0.82rem', color: 'var(--text-muted)', lineHeight: 1.5 }}>
                    {recapResult.eventosPrecedentes?.map((ev, i) => (
                      <li key={i}>{ev}</li>
                    ))}
                  </ul>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* TAB 4: TRIVIA & QUIZ DO ACERVO */}
      {activeTab === 'quiz' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
              <div>
                <h2 style={{ fontSize: '1.25rem', fontWeight: 700, color: '#fff', margin: '0 0 4px', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <Trophy size={20} color="#eab308" />
                  Trivia & Quiz Interativo do Acervo
                </h2>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.86rem', margin: 0 }}>
                  Perguntas e respostas dinâmicas baseadas estritamente nas HQs e personagens que você possui na sua coleção.
                </p>
              </div>

              <button
                type="button"
                onClick={handleGerarQuiz}
                disabled={isLoadingQuiz}
                className="btn-secondary"
                style={{ display: 'flex', alignItems: 'center', gap: 6, padding: '8px 16px', fontSize: '0.84rem' }}
              >
                <RefreshCw size={14} className={isLoadingQuiz ? 'animate-spin' : ''} />
                Gerar Novo Quiz
              </button>
            </div>
          </div>

          {isLoadingQuiz && (
            <div style={{ textAlign: 'center', padding: '60px 0', color: 'var(--text-muted)' }}>
              <Loader2 size={36} className="animate-spin" style={{ margin: '0 auto 12px', color: '#eab308' }} />
              <p style={{ fontSize: '1rem', fontWeight: 600 }}>Elaborando perguntas desafiadoras sobre suas HQs...</p>
            </div>
          )}

          {!isLoadingQuiz && quizResult && !quizFinished && (
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 14,
              padding: 28,
              maxWidth: 800,
              margin: '0 auto',
              width: '100%'
            }}>
              {/* Quiz Header & Progress */}
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
                <span style={{ fontSize: '0.85rem', fontWeight: 700, color: 'var(--text-muted)' }}>
                  Pergunta {currentQuestionIndex + 1} de {quizResult.perguntas.length}
                </span>
                <span style={{
                  background: 'rgba(234, 179, 8, 0.15)',
                  color: '#eab308',
                  padding: '4px 12px',
                  borderRadius: 20,
                  fontSize: '0.8rem',
                  fontWeight: 800
                }}>
                  Pontuação: {score}
                </span>
              </div>

              {/* Question */}
              <h3 style={{ fontSize: '1.25rem', fontWeight: 700, color: '#fff', margin: '0 0 10px', lineHeight: 1.4 }}>
                {quizResult.perguntas[currentQuestionIndex].pergunta}
              </h3>
              <div style={{ fontSize: '0.78rem', color: 'var(--accent-cyan)', marginBottom: 20 }}>
                ✦ {quizResult.perguntas[currentQuestionIndex].hqReferencia}
              </div>

              {/* Options */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: 10, marginBottom: 20 }}>
                {quizResult.perguntas[currentQuestionIndex].opcoes.map((opcao, optIdx) => {
                  const isSelected = selectedAnswer === optIdx;
                  const isCorrect = optIdx === quizResult.perguntas[currentQuestionIndex].respostaCorretaIndex;
                  const showResult = selectedAnswer !== null;

                  let bgColor = 'var(--bg-card-hover)';
                  let borderColor = 'var(--border-subtle)';
                  let textColor = '#fff';

                  if (showResult) {
                    if (isCorrect) {
                      bgColor = 'rgba(16, 185, 129, 0.2)';
                      borderColor = '#10b981';
                      textColor = '#10b981';
                    } else if (isSelected) {
                      bgColor = 'rgba(239, 68, 68, 0.2)';
                      borderColor = '#ef4444';
                      textColor = '#ef4444';
                    }
                  }

                  return (
                    <button
                      key={optIdx}
                      type="button"
                      disabled={showResult}
                      onClick={() => handleAnswerClick(optIdx)}
                      style={{
                        padding: '14px 18px',
                        borderRadius: 8,
                        background: bgColor,
                        border: `1px solid ${borderColor}`,
                        color: textColor,
                        textAlign: 'left',
                        fontSize: '0.92rem',
                        fontWeight: 600,
                        cursor: showResult ? 'default' : 'pointer',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        transition: 'all 0.15s ease'
                      }}
                    >
                      <span>{String.fromCharCode(65 + optIdx)}) {opcao}</span>
                      {showResult && isCorrect && <Check size={18} color="#10b981" />}
                    </button>
                  );
                })}
              </div>

              {/* Explanation & Next */}
              {selectedAnswer !== null && (
                <div style={{
                  padding: 16,
                  borderRadius: 8,
                  background: 'rgba(255,255,255,0.03)',
                  border: '1px solid var(--border-subtle)',
                  marginBottom: 20
                }}>
                  <div style={{ fontSize: '0.8rem', fontWeight: 700, color: '#fff', marginBottom: 4 }}>
                    Explicação Curatorial:
                  </div>
                  <p style={{ margin: 0, fontSize: '0.85rem', color: 'var(--text-muted)', lineHeight: 1.5 }}>
                    {quizResult.perguntas[currentQuestionIndex].explicacao}
                  </p>
                </div>
              )}

              {selectedAnswer !== null && (
                <button
                  type="button"
                  onClick={handleNextQuestion}
                  className="btn-primary"
                  style={{ width: '100%', padding: '12px', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 8 }}
                >
                  {currentQuestionIndex + 1 < quizResult.perguntas.length ? 'Próxima Pergunta' : 'Ver Resultado Final'}
                  <ArrowRight size={16} />
                </button>
              )}
            </div>
          )}

          {quizFinished && (
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 14,
              padding: 40,
              maxWidth: 600,
              margin: '0 auto',
              textAlign: 'center'
            }}>
              <Trophy size={48} color="#eab308" style={{ margin: '0 auto 16px' }} />
              <h2 style={{ fontSize: '1.8rem', fontWeight: 800, color: '#fff', margin: '0 0 8px' }}>
                Quiz Concluído!
              </h2>
              <p style={{ fontSize: '1.2rem', color: 'var(--text-muted)', margin: '0 0 24px' }}>
                Você acertou <strong style={{ color: '#10b981' }}>{score}</strong> de <strong>{quizResult?.perguntas.length}</strong> perguntas sobre o seu acervo!
              </p>

              <button
                type="button"
                onClick={handleGerarQuiz}
                className="btn-primary"
                style={{ padding: '12px 28px', fontSize: '0.95rem' }}
              >
                Jogar Outra Rodada
              </button>
            </div>
          )}
        </div>
      )}

      {/* TAB 5: CURADOR VIRTUAL (CHATBOT COM RAG EM MEMÓRIA) */}
      {activeTab === 'chat' && (
        <div style={{
          background: 'var(--bg-card)',
          border: '1px solid var(--border-subtle)',
          borderRadius: 14,
          display: 'flex',
          flexDirection: 'column',
          height: 650,
          overflow: 'hidden'
        }}>
          {/* Chat Header */}
          <div style={{
            padding: '16px 20px',
            borderBottom: '1px solid var(--border-subtle)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            background: 'rgba(0,0,0,0.2)'
          }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <div style={{
                width: 36,
                height: 36,
                borderRadius: '50%',
                background: 'linear-gradient(135deg, #a855f7, #06b6d4)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#fff'
              }}>
                <Bot size={20} />
              </div>
              <div>
                <h3 style={{ fontSize: '1rem', fontWeight: 700, color: '#fff', margin: 0 }}>
                  Curador Virtual • RAG em Memória
                </h3>
                <span style={{ fontSize: '0.75rem', color: '#10b981' }}>
                  ● Conectado ao catálogo de {userHqs.length} HQs
                </span>
              </div>
            </div>

            {/* Quick Prompts */}
            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
              {[
                'Sugira uma HQ sombria para ler hoje',
                'Quantas HQs lidas eu tenho?',
                'Qual meu autor ou série mais recorrente?'
              ].map(qp => (
                <button
                  key={qp}
                  type="button"
                  onClick={() => handleSendChat(qp)}
                  style={{
                    background: 'rgba(255,255,255,0.05)',
                    border: '1px solid var(--border-subtle)',
                    padding: '4px 10px',
                    borderRadius: 16,
                    fontSize: '0.74rem',
                    color: 'var(--text-muted)',
                    cursor: 'pointer'
                  }}
                >
                  {qp}
                </button>
              ))}
            </div>
          </div>

          {/* Messages Body */}
          <div style={{
            flex: 1,
            overflowY: 'auto',
            padding: '20px',
            display: 'flex',
            flexDirection: 'column',
            gap: 16
          }}>
            {chatMessages.map((msg, idx) => (
              <div
                key={idx}
                style={{
                  display: 'flex',
                  justifyContent: msg.remetente === 'user' ? 'flex-end' : 'flex-start'
                }}
              >
                <div style={{
                  maxWidth: '75%',
                  padding: '12px 18px',
                  borderRadius: 12,
                  background: msg.remetente === 'user' ? 'var(--accent-purple)' : 'var(--bg-card-hover)',
                  color: '#fff',
                  fontSize: '0.9rem',
                  lineHeight: 1.5,
                  whiteSpace: 'pre-wrap',
                  boxShadow: '0 4px 12px rgba(0,0,0,0.15)'
                }}>
                  {msg.origemMotor === 'JEV_SYSTEM1_LOCAL' && (
                    <div style={{
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: 4,
                      fontSize: '0.72rem',
                      fontWeight: 700,
                      color: '#fbbf24',
                      marginBottom: 8,
                      background: 'rgba(234, 179, 8, 0.12)',
                      padding: '2px 8px',
                      borderRadius: 10,
                      border: '1px solid rgba(234, 179, 8, 0.3)'
                    }}>
                      <Zap size={11} /> JEV Engine (Sistema 1 Local • ~2ms)
                    </div>
                  )}
                  <div>{msg.texto}</div>
                </div>
              </div>
            ))}
            {isSendingChat && (
              <div style={{ display: 'flex', justifyContent: 'flex-start' }}>
                <div style={{
                  padding: '10px 16px',
                  borderRadius: 12,
                  background: 'var(--bg-card-hover)',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  color: 'var(--text-muted)',
                  fontSize: '0.85rem'
                }}>
                  <Loader2 size={16} className="animate-spin" /> Consultando acervo...
                </div>
              </div>
            )}
            <div ref={chatEndRef} />
          </div>

          {/* Chat Input Bar */}
          <form
            onSubmit={(e) => { e.preventDefault(); handleSendChat(); }}
            style={{
              padding: '14px 20px',
              borderTop: '1px solid var(--border-subtle)',
              display: 'flex',
              gap: 10,
              background: 'rgba(0,0,0,0.2)'
            }}
          >
            <input
              type="text"
              placeholder="Pergunte sobre seu acervo, sugestões de leitura ou detalhes das suas HQs..."
              value={chatInput}
              onChange={(e) => setChatInput(e.target.value)}
              style={{
                flex: 1,
                padding: '12px 16px',
                background: 'var(--bg-card-hover)',
                border: '1px solid var(--border-subtle)',
                borderRadius: 8,
                color: '#fff',
                fontSize: '0.92rem'
              }}
            />
            <button
              type="submit"
              className="btn-primary"
              disabled={isSendingChat || !chatInput.trim()}
              style={{ padding: '12px 20px', display: 'flex', alignItems: 'center', gap: 6 }}
            >
              <Send size={16} />
              Enviar
            </button>
          </form>
        </div>
      )}

      {/* TAB 6: COMANDOS CRUD EM LINGUAGEM NATURAL */}
      {activeTab === 'crud' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 700, color: '#fff', margin: '0 0 6px', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Zap size={20} color="#06b6d4" />
              Assistente de Comandos CRUD em Linguagem Natural
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', margin: '0 0 16px' }}>
              Interpreta comandos livres, extrai parâmetros estruturados e gera uma prévia interativa para você confirmar antes de gravar no banco de dados.
            </p>

            <div style={{ display: 'flex', gap: 8, flexWrap: 'wrap', marginBottom: 16 }}>
              <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 600 }}>Exemplos de comandos:</span>
              {[
                'marque Batman Ano Um como lido',
                'adicione Sandman 1 da Panini na estante',
                'coloque Demolidor A Queda de Murdock na lista de desejos',
                'desmarque Watchmen como lido'
              ].map(cmd => (
                <button
                  key={cmd}
                  type="button"
                  onClick={() => { setCrudInput(cmd); }}
                  style={{
                    background: 'rgba(255,255,255,0.05)',
                    border: '1px solid var(--border-subtle)',
                    padding: '4px 10px',
                    borderRadius: 16,
                    fontSize: '0.78rem',
                    color: 'var(--text-primary)',
                    cursor: 'pointer'
                  }}
                >
                  "{cmd}"
                </button>
              ))}
            </div>

            <form onSubmit={handleGerarPreviewCrud} style={{ display: 'flex', gap: 10 }}>
              <input
                type="text"
                placeholder="Digite o comando livre (ex: marque Demolidor 1 como lido, adicione X na lista de desejos)..."
                value={crudInput}
                onChange={(e) => setCrudInput(e.target.value)}
                style={{
                  flex: 1,
                  padding: '12px 16px',
                  background: 'var(--bg-card-hover)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 8,
                  color: '#fff',
                  fontSize: '0.95rem'
                }}
              />
              <button
                type="submit"
                className="btn-primary"
                disabled={isLoadingCrudPreview || !crudInput.trim()}
                style={{ padding: '12px 22px', display: 'flex', alignItems: 'center', gap: 8 }}
              >
                {isLoadingCrudPreview ? <Loader2 size={16} className="animate-spin" /> : <Zap size={16} />}
                Interpretar Ordem
              </button>
            </form>
          </div>

          {/* Interactive Preview Card */}
          {crudPreview && (
            <div style={{
              background: 'linear-gradient(135deg, rgba(6, 182, 212, 0.08) 0%, rgba(15, 23, 42, 0.9) 100%)',
              border: '1px solid rgba(6, 182, 212, 0.3)',
              borderRadius: 14,
              padding: 24
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
                <div>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 8, flexWrap: 'wrap' }}>
                    <span style={{ fontSize: '0.75rem', fontWeight: 800, color: 'var(--accent-cyan)', textTransform: 'uppercase' }}>
                      ● PRÉVIA DA AÇÃO IDENTIFICADA
                    </span>
                    {crudPreview.origemMotor === 'JEV_SYSTEM1_LOCAL' && (
                      <span style={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: 4,
                        padding: '2px 8px',
                        borderRadius: 12,
                        fontSize: '0.72rem',
                        fontWeight: 700,
                        background: 'rgba(234, 179, 8, 0.15)',
                        border: '1px solid rgba(234, 179, 8, 0.4)',
                        color: '#fbbf24'
                      }}>
                        <Zap size={11} /> JEV Engine (Local • {crudPreview.tempoProcessamentoMs ?? 2}ms)
                      </span>
                    )}
                  </div>
                  <h3 style={{ fontSize: '1.3rem', fontWeight: 800, color: '#fff', margin: '4px 0 0' }}>
                    {crudPreview.interpretacaoHumana}
                  </h3>
                </div>

                <span style={{
                  padding: '4px 12px',
                  borderRadius: 20,
                  fontSize: '0.78rem',
                  fontWeight: 700,
                  background: 'rgba(16, 185, 129, 0.15)',
                  color: '#10b981'
                }}>
                  Ação: {crudPreview.acao}
                </span>
              </div>

              {/* Parsed Attributes */}
              <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
                gap: 12,
                background: 'rgba(0,0,0,0.25)',
                padding: 16,
                borderRadius: 8,
                marginBottom: 20
              }}>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>TÍTULO:</span>
                  <div style={{ fontSize: '0.92rem', fontWeight: 700, color: '#fff' }}>{crudPreview.tituloAlvo || '-'}</div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>EDITORA:</span>
                  <div style={{ fontSize: '0.92rem', fontWeight: 700, color: '#fff' }}>{crudPreview.editoraAlvo || 'Padrão'}</div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>EDIÇÃO:</span>
                  <div style={{ fontSize: '0.92rem', fontWeight: 700, color: '#fff' }}>#{crudPreview.edicaoAlvo || 1}</div>
                </div>
                <div>
                  <span style={{ fontSize: '0.72rem', color: 'var(--text-muted)', fontWeight: 600 }}>REGISTRO EXISTENTE:</span>
                  <div style={{ fontSize: '0.92rem', fontWeight: 700, color: crudPreview.hqIdAlvo ? '#10b981' : '#f59e0b' }}>
                    {crudPreview.hqIdAlvo ? 'Localizado no Acervo' : 'Novo Registro'}
                  </div>
                </div>
              </div>

              {/* Actions */}
              <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end' }}>
                <button
                  type="button"
                  onClick={() => setCrudPreview(null)}
                  className="btn-secondary"
                  style={{ padding: '10px 18px', fontSize: '0.85rem' }}
                >
                  Cancelar
                </button>
                <button
                  type="button"
                  onClick={handleExecutarCrud}
                  disabled={isExecutingCrud}
                  className="btn-primary"
                  style={{ padding: '10px 24px', fontSize: '0.85rem', display: 'flex', alignItems: 'center', gap: 8 }}
                >
                  {isExecutingCrud ? <Loader2 size={16} className="animate-spin" /> : <Check size={16} />}
                  Confirmar e Executar no Banco
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* TAB 7: IMPORTAÇÃO EM LOTE ASSISTIDA */}
      {activeTab === 'lote' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 20 }}>
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <h2 style={{ fontSize: '1.25rem', fontWeight: 700, color: '#fff', margin: '0 0 6px', display: 'flex', alignItems: 'center', gap: 8 }}>
              <Layers size={20} color="#a855f7" />
              Importação em Lote Assistida
            </h2>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.88rem', margin: '0 0 16px' }}>
              Cole listas desestruturadas de texto e intervalos numéricos (ex: "volumes 1 ao 15 de Berserk da Panini, Spawn 1 a 6 Abril"). A IA desdobrará cada edição e preencherá metadados prontos para gravação.
            </p>

            <form onSubmit={handleGerarLote} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
              <textarea
                rows={4}
                required
                placeholder="Cole aqui seu texto livre. Exemplo:&#10;Berserk volumes 1 ao 12 Panini&#10;Akira 1 a 6 JBC&#10;Demolidor A Queda de Murdock Panini"
                value={loteTexto}
                onChange={(e) => setLoteTexto(e.target.value)}
                style={{
                  width: '100%',
                  padding: '12px 16px',
                  background: 'var(--bg-card-hover)',
                  border: '1px solid var(--border-subtle)',
                  borderRadius: 8,
                  color: '#fff',
                  fontSize: '0.9rem',
                  resize: 'vertical'
                }}
              />

              <button
                type="submit"
                className="btn-primary"
                disabled={isLoadingLote || !loteTexto.trim()}
                style={{ alignSelf: 'flex-start', padding: '10px 22px', display: 'flex', alignItems: 'center', gap: 8 }}
              >
                {isLoadingLote ? <Loader2 size={16} className="animate-spin" /> : <Layers size={16} />}
                Interpretar Lista em Lote
              </button>
            </form>
          </div>

          {/* Lote Preview Table */}
          {lotePreview && (
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 14,
              padding: 24
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16, flexWrap: 'wrap', gap: 12 }}>
                <div>
                  <h3 style={{ fontSize: '1.15rem', fontWeight: 700, color: '#fff', margin: 0 }}>
                    {lotePreview.totalDetectados} Quadrinho(s) Identificados
                  </h3>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
                    Revise os itens e selecione quais deseja gravar.
                  </span>
                </div>

                <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                  <label style={{ fontSize: '0.85rem', color: 'var(--text-muted)', display: 'flex', alignItems: 'center', gap: 6 }}>
                    Destino:
                    <select
                      value={loteDestino}
                      onChange={(e) => setLoteDestino(e.target.value as any)}
                      style={{
                        padding: '6px 10px',
                        background: 'var(--bg-card-hover)',
                        border: '1px solid var(--border-subtle)',
                        borderRadius: 6,
                        color: '#fff',
                        fontSize: '0.85rem'
                      }}
                    >
                      <option value="colecao">Coleção Principal</option>
                      <option value="desejos">Lista de Desejos</option>
                    </select>
                  </label>

                  <button
                    type="button"
                    onClick={handleSalvarLote}
                    disabled={isSavingLote}
                    className="btn-primary"
                    style={{ padding: '8px 18px', fontSize: '0.85rem', display: 'flex', alignItems: 'center', gap: 6 }}
                  >
                    {isSavingLote ? <Loader2 size={14} className="animate-spin" /> : <Check size={14} />}
                    Salvar Selecionados
                  </button>
                </div>
              </div>

              {/* Table */}
              <div style={{ overflowX: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '0.88rem' }}>
                  <thead>
                    <tr style={{ borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                      <th style={{ padding: '10px 12px', width: 40 }}>Sel.</th>
                      <th style={{ padding: '10px 12px' }}>Título</th>
                      <th style={{ padding: '10px 12px' }}>Editora</th>
                      <th style={{ padding: '10px 12px' }}>Edição</th>
                      <th style={{ padding: '10px 12px' }}>Preço</th>
                      <th style={{ padding: '10px 12px' }}>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lotePreview.itens.map(item => (
                      <tr
                        key={item.idTemp}
                        onClick={() => handleToggleItemLote(item.idTemp)}
                        style={{
                          borderBottom: '1px solid rgba(255,255,255,0.04)',
                          cursor: 'pointer',
                          background: item.selecionado ? 'rgba(168, 85, 247, 0.05)' : 'transparent'
                        }}
                      >
                        <td style={{ padding: '10px 12px' }}>
                          {item.selecionado ? <CheckSquare size={16} color="var(--accent-purple)" /> : <Square size={16} color="var(--text-muted)" />}
                        </td>
                        <td style={{ padding: '10px 12px', fontWeight: 600, color: '#fff' }}>
                          {item.titulo}
                        </td>
                        <td style={{ padding: '10px 12px', color: 'var(--text-muted)' }}>
                          {item.editora}
                        </td>
                        <td style={{ padding: '10px 12px', color: '#fff' }}>
                          #{item.numeroEdicao}
                        </td>
                        <td style={{ padding: '10px 12px', color: 'var(--text-muted)' }}>
                          {item.precoSugerido}
                        </td>
                        <td style={{ padding: '10px 12px' }}>
                          {item.jaExisteNaColecao ? (
                            <span style={{ fontSize: '0.72rem', color: '#f59e0b', fontWeight: 700 }}>
                              Já na Coleção
                            </span>
                          ) : (
                            <span style={{ fontSize: '0.72rem', color: '#10b981', fontWeight: 700 }}>
                              Novo
                            </span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      )}

      {/* TAB 8: RELATÓRIO DE IA & OBSERVABILIDADE (OPENTELEMETRY) */}
      {activeTab === 'telemetria' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
          {/* Header & Badges */}
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 16, marginBottom: 16 }}>
              <div>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginBottom: 6 }}>
                  <span style={{
                    background: 'rgba(16, 185, 129, 0.15)',
                    border: '1px solid rgba(16, 185, 129, 0.3)',
                    color: '#10b981',
                    padding: '3px 8px',
                    borderRadius: 6,
                    fontSize: '0.72rem',
                    fontWeight: 700,
                    textTransform: 'uppercase'
                  }}>
                    OpenTelemetry GenAI Semantic Conventions
                  </span>
                  <span style={{
                    background: 'rgba(168, 85, 247, 0.15)',
                    border: '1px solid rgba(168, 85, 247, 0.3)',
                    color: 'var(--accent-purple)',
                    padding: '3px 8px',
                    borderRadius: 6,
                    fontSize: '0.72rem',
                    fontWeight: 700
                  }}>
                    Tracer: ProjetoHQ.IA.Observability
                  </span>
                </div>
                <h2 style={{ fontSize: '1.45rem', fontWeight: 800, margin: '0 0 6px', color: '#fff' }}>
                  Relatório de Observabilidade de IA
                </h2>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem', margin: 0, maxWidth: 800 }}>
                  Métricas em tempo real de consumo de tokens, latência P95, custos financeiros e eficiência do Motor Híbrido Local (JEV Engine) vs. Gemini 2.5 Flash.
                </p>
              </div>

              <div style={{ display: 'flex', gap: 10 }}>
                <button
                  type="button"
                  onClick={carregarTelemetria}
                  disabled={isLoadingTelemetria}
                  style={{
                    background: 'rgba(255,255,255,0.06)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 8,
                    padding: '9px 16px',
                    color: '#fff',
                    fontSize: '0.82rem',
                    fontWeight: 600,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    cursor: 'pointer'
                  }}
                >
                  <RefreshCw size={15} className={isLoadingTelemetria ? 'animate-spin' : ''} />
                  Atualizar Dados
                </button>
                <button
                  type="button"
                  onClick={handleLimparTelemetria}
                  disabled={isLoadingTelemetria}
                  style={{
                    background: 'rgba(239, 68, 68, 0.08)',
                    border: '1px solid rgba(239, 68, 68, 0.25)',
                    borderRadius: 8,
                    padding: '9px 16px',
                    color: '#ef4444',
                    fontSize: '0.82rem',
                    fontWeight: 600,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 8,
                    cursor: 'pointer'
                  }}
                >
                  <Trash2 size={15} />
                  Reiniciar Métricas
                </button>
              </div>
            </div>
          </div>

          {/* 6 KPI Cards */}
          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(320px, 1fr))',
            gap: 16
          }}>
            {/* KPI 1: Total Chamadas & Sucesso */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(168, 85, 247, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(168, 85, 247, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Total Chamadas de IA
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(168, 85, 247, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Activity size={18} color="var(--accent-purple)" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#fff', lineHeight: 1 }}>
                  {telemetriaResumo?.totalChamadas ?? 0}
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  <span style={{ color: 'var(--accent-cyan)', fontWeight: 600 }}>{telemetriaResumo?.totalChamadasGemini ?? 0}</span> Gemini Nuvem | <span style={{ color: '#10b981', fontWeight: 600 }}>{telemetriaResumo?.totalChamadasJev ?? 0}</span> JEV Local
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Taxa de Confiabilidade</span>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: '#10b981', background: 'rgba(16, 185, 129, 0.1)', padding: '2px 8px', borderRadius: 4 }}>
                  {telemetriaResumo?.taxaSucessoPercentual ?? 100}% Sucesso
                </span>
              </div>
            </div>

            {/* KPI 2: Tokens Consumidos */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(6, 182, 212, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(6, 182, 212, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Tokens Consumidos
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(6, 182, 212, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Cpu size={18} color="var(--accent-cyan)" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#fff', lineHeight: 1 }}>
                  {(telemetriaResumo?.totalTokens ?? 0).toLocaleString()}
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  Prompt: {(telemetriaResumo?.totalPromptTokens ?? 0).toLocaleString()} | Resposta: {(telemetriaResumo?.totalCompletionTokens ?? 0).toLocaleString()}
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Ferramentas Function Calling</span>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: 'var(--accent-cyan)' }}>
                  {telemetriaResumo?.totalFerramentas ?? 0} acionamentos
                </span>
              </div>
            </div>

            {/* KPI 3: Latência Média & P95 */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(245, 158, 11, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(245, 158, 11, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Latência Média & P95
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(245, 158, 11, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Clock size={18} color="#f59e0b" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#fff', lineHeight: 1 }}>
                  {telemetriaResumo?.latenciaMediaMs ?? 0} <span style={{ fontSize: '1rem', color: 'var(--text-muted)' }}>ms</span>
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  Gemini Nuvem: <span style={{ color: '#f59e0b', fontWeight: 600 }}>{telemetriaResumo?.latenciaMediaGeminiMs ?? 0}ms</span> | JEV Local: <span style={{ color: '#10b981', fontWeight: 600 }}>{telemetriaResumo?.latenciaMediaJevMs ?? 0}ms</span>
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>P95 (95% dos disparos abaixo de)</span>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: '#f59e0b' }}>
                  {telemetriaResumo?.latenciaP95Ms ?? 0} ms
                </span>
              </div>
            </div>

            {/* KPI 4: Custo Financeiro Total */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(16, 185, 129, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(16, 185, 129, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Custo Financeiro Total
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(16, 185, 129, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <DollarSign size={18} color="#10b981" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#10b981', lineHeight: 1 }}>
                  US$ {(telemetriaResumo?.custoTotalUSD ?? 0).toFixed(4)}
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  Aproximadamente <strong style={{ color: '#fff' }}>R$ {(telemetriaResumo?.custoTotalBRL ?? 0).toFixed(2)}</strong> (câmbio R$ 5,65)
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Tabela Gemini 2.5 Flash</span>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                  $0.075 / $0.300 por 1M
                </span>
              </div>
            </div>

            {/* KPI 5: Custo por Requisição Bem-Sucedida */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(59, 130, 246, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(59, 130, 246, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Custo / Req. Bem-Sucedida
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(59, 130, 246, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <TrendingUp size={18} color="#3b82f6" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#3b82f6', lineHeight: 1 }}>
                  US$ {(telemetriaResumo?.custoPorRequisicaoSucessoUSD ?? 0).toFixed(6)}
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  Custo médio de <strong style={{ color: '#fff' }}>R$ {(telemetriaResumo?.custoPorRequisicaoSucessoBRL ?? 0).toFixed(4)}</strong> por resposta entregue
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Métrica de Eficiência</span>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: '#3b82f6' }}>
                  Alta Eficiência
                </span>
              </div>
            </div>

            {/* KPI 6: Economia do JEV Engine (Local) */}
            <div style={{
              background: 'linear-gradient(135deg, rgba(234, 88, 12, 0.08) 0%, rgba(15, 23, 42, 0.6) 100%)',
              border: '1px solid rgba(234, 88, 12, 0.25)',
              borderRadius: 14,
              padding: 20,
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between'
            }}>
              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
                  <span style={{ fontSize: '0.78rem', color: 'var(--text-muted)', fontWeight: 700, textTransform: 'uppercase' }}>
                    Economia JEV Engine (Local)
                  </span>
                  <div style={{ width: 34, height: 34, borderRadius: 8, background: 'rgba(234, 88, 12, 0.2)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Zap size={18} color="#ea580c" />
                  </div>
                </div>
                <div style={{ fontSize: '2.1rem', fontWeight: 800, color: '#ea580c', lineHeight: 1 }}>
                  US$ {(telemetriaResumo?.totalEconomizadoJevUSD ?? 0).toFixed(4)}
                </div>
                <div style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginTop: 8 }}>
                  <strong style={{ color: '#fff' }}>R$ {(telemetriaResumo?.totalEconomizadoJevBRL ?? 0).toFixed(2)}</strong> poupados via Sistema 1 Local em ~2ms
                </div>
              </div>
              <div style={{ marginTop: 14, paddingTop: 12, borderTop: '1px solid rgba(255,255,255,0.06)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Tokens Poupados da Nuvem</span>
                <span style={{ fontSize: '0.8rem', fontWeight: 700, color: '#ea580c' }}>
                  {(telemetriaResumo?.tokensEconomizadosJev ?? 0).toLocaleString()} tokens
                </span>
              </div>
            </div>
          </div>

          {/* Breakdown por Agente / Funcionalidade */}
          {telemetriaResumo && telemetriaResumo.distribuicaoPorFuncionalidade.length > 0 && (
            <div style={{
              background: 'var(--bg-card)',
              border: '1px solid var(--border-subtle)',
              borderRadius: 14,
              padding: 24
            }}>
              <h3 style={{ fontSize: '1.15rem', fontWeight: 700, margin: '0 0 16px', color: '#fff', display: 'flex', alignItems: 'center', gap: 8 }}>
                <BarChart3 size={18} color="var(--accent-purple)" />
                Distribuição por Agente & Funcionalidade
              </h3>

              <div style={{ display: 'flex', flexDirection: 'column', gap: 14 }}>
                {telemetriaResumo.distribuicaoPorFuncionalidade.map((item, idx) => (
                  <div key={idx} style={{
                    background: 'rgba(255,255,255,0.02)',
                    border: '1px solid var(--border-subtle)',
                    borderRadius: 10,
                    padding: '14px 18px'
                  }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 10, marginBottom: 8 }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                        <span style={{ fontWeight: 700, color: '#fff', fontSize: '0.92rem' }}>
                          {item.funcionalidade}
                        </span>
                        <span style={{
                          fontSize: '0.72rem',
                          padding: '2px 8px',
                          borderRadius: 4,
                          background: 'rgba(168, 85, 247, 0.15)',
                          color: 'var(--accent-purple)',
                          fontWeight: 600
                        }}>
                          {item.chamadas} chamadas ({item.porcentagemUso}%)
                        </span>
                      </div>

                      <div style={{ display: 'flex', alignItems: 'center', gap: 16, fontSize: '0.82rem', color: 'var(--text-muted)' }}>
                        <span>Tokens: <strong style={{ color: '#fff' }}>{item.tokens.toLocaleString()}</strong></span>
                        <span>Latência Média: <strong style={{ color: item.latenciaMediaMs < 100 ? '#10b981' : '#f59e0b' }}>{item.latenciaMediaMs}ms</strong></span>
                        <span>Custo: <strong style={{ color: '#10b981' }}>US$ {item.custoUSD.toFixed(5)}</strong></span>
                      </div>
                    </div>

                    {/* Progress Bar */}
                    <div style={{ width: '100%', height: 6, background: 'rgba(255,255,255,0.06)', borderRadius: 3, overflow: 'hidden' }}>
                      <div style={{
                        width: `${Math.min(100, Math.max(4, item.porcentagemUso))}%`,
                        height: '100%',
                        background: 'linear-gradient(90deg, var(--accent-purple), var(--accent-cyan))',
                        borderRadius: 3
                      }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Spans / Logs de Auditoria OpenTelemetry */}
          <div style={{
            background: 'var(--bg-card)',
            border: '1px solid var(--border-subtle)',
            borderRadius: 14,
            padding: 24
          }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 14, marginBottom: 16 }}>
              <div>
                <h3 style={{ fontSize: '1.15rem', fontWeight: 700, margin: '0 0 4px', color: '#fff', display: 'flex', alignItems: 'center', gap: 8 }}>
                  <History size={18} color="var(--accent-cyan)" />
                  Auditoria de Spans GenAI (Últimas Chamadas)
                </h3>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.82rem', margin: 0 }}>
                  Eventos de telemetria capturados no padrão OpenTelemetry com tags semânticas e latência exata.
                </p>
              </div>

              {/* Filtro por Motor */}
              <div style={{ display: 'flex', gap: 6, background: 'rgba(255,255,255,0.04)', padding: 4, borderRadius: 8, border: '1px solid var(--border-subtle)' }}>
                {[
                  { id: 'todos', label: 'Todos' },
                  { id: 'gemini', label: 'Gemini Nuvem' },
                  { id: 'jev', label: 'JEV Engine Local' }
                ].map(f => (
                  <button
                    key={f.id}
                    type="button"
                    onClick={() => setFiltroMotor(f.id as any)}
                    style={{
                      background: filtroMotor === f.id ? 'var(--accent-purple)' : 'transparent',
                      color: filtroMotor === f.id ? '#fff' : 'var(--text-muted)',
                      border: 'none',
                      borderRadius: 6,
                      padding: '4px 10px',
                      fontSize: '0.76rem',
                      fontWeight: 600,
                      cursor: 'pointer'
                    }}
                  >
                    {f.label}
                  </button>
                ))}
              </div>
            </div>

            <div style={{ overflowX: 'auto' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.82rem', textAlign: 'left' }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid var(--border-subtle)', color: 'var(--text-muted)' }}>
                    <th style={{ padding: '10px 12px' }}>Data / Hora</th>
                    <th style={{ padding: '10px 12px' }}>Funcionalidade</th>
                    <th style={{ padding: '10px 12px' }}>Operação (gen_ai)</th>
                    <th style={{ padding: '10px 12px' }}>Motor / Modelo</th>
                    <th style={{ padding: '10px 12px' }}>Tokens</th>
                    <th style={{ padding: '10px 12px' }}>Latência</th>
                    <th style={{ padding: '10px 12px' }}>Custo Est.</th>
                    <th style={{ padding: '10px 12px' }}>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {telemetriaHistorico
                    .filter(log => {
                      if (filtroMotor === 'gemini') return log.origemMotor === 'GEMINI_SYSTEM2_CLOUD';
                      if (filtroMotor === 'jev') return log.origemMotor === 'JEV_SYSTEM1_LOCAL';
                      return true;
                    })
                    .map(log => {
                      const isJev = log.origemMotor === 'JEV_SYSTEM1_LOCAL';
                      return (
                        <tr
                          key={log.id}
                          style={{
                            borderBottom: '1px solid rgba(255,255,255,0.04)',
                            background: isJev ? 'rgba(16, 185, 129, 0.02)' : 'transparent'
                          }}
                        >
                          <td style={{ padding: '10px 12px', color: 'var(--text-muted)', whiteSpace: 'nowrap' }}>
                            {new Date(log.dataHora).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
                          </td>
                          <td style={{ padding: '10px 12px', fontWeight: 600, color: '#fff' }}>
                            {log.funcionalidade}
                          </td>
                          <td style={{ padding: '10px 12px', color: 'var(--text-muted)', fontFamily: 'monospace', fontSize: '0.78rem' }}>
                            {log.operacao}
                          </td>
                          <td style={{ padding: '10px 12px' }}>
                            {isJev ? (
                              <span style={{
                                background: 'rgba(16, 185, 129, 0.15)',
                                color: '#10b981',
                                padding: '2px 8px',
                                borderRadius: 4,
                                fontSize: '0.72rem',
                                fontWeight: 700
                              }}>
                                JEV Local (0ms $)
                              </span>
                            ) : (
                              <span style={{
                                background: 'rgba(168, 85, 247, 0.15)',
                                color: 'var(--accent-purple)',
                                padding: '2px 8px',
                                borderRadius: 4,
                                fontSize: '0.72rem',
                                fontWeight: 700
                              }}>
                                {log.modelo}
                              </span>
                            )}
                          </td>
                          <td style={{ padding: '10px 12px', color: '#fff' }}>
                            {log.totalTokens > 0 ? (
                              <span>{log.totalTokens} <small style={{ color: 'var(--text-muted)' }}>({log.promptTokens}/{log.completionTokens})</small></span>
                            ) : (
                              <span style={{ color: 'var(--text-muted)' }}>-</span>
                            )}
                          </td>
                          <td style={{ padding: '10px 12px' }}>
                            <span style={{
                              color: log.latenciaMs < 20 ? '#10b981' : log.latenciaMs < 2000 ? '#06b6d4' : '#f59e0b',
                              fontWeight: 700
                            }}>
                              {log.latenciaMs} ms
                            </span>
                          </td>
                          <td style={{ padding: '10px 12px', color: isJev ? '#10b981' : '#fff' }}>
                            {isJev ? (
                              <span title={`Economizou US$ ${log.custoEconomizadoUSD.toFixed(5)}`}>R$ 0,00</span>
                            ) : (
                              <span>US$ {log.custoEstimadoUSD.toFixed(5)}</span>
                            )}
                          </td>
                          <td style={{ padding: '10px 12px' }}>
                            {log.sucesso ? (
                              <span style={{ color: '#10b981', display: 'inline-flex', alignItems: 'center', gap: 4, fontWeight: 700 }}>
                                <Check size={14} /> 200 OK
                              </span>
                            ) : (
                              <span style={{ color: '#ef4444', display: 'inline-flex', alignItems: 'center', gap: 4, fontWeight: 700 }} title={log.mensagemErro}>
                                <AlertTriangle size={14} /> Erro
                              </span>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
