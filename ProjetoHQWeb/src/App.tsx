import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Navbar } from './components/Navbar';
import { HomePage } from './pages/HomePage';
import { DesejosPage } from './pages/DesejosPage';
import { LeiturasPage } from './pages/LeiturasPage';
import { SettingsPage } from './pages/SettingsPage';

import { EditorasPage } from './pages/EditorasPage';
import { FrasesPage } from './pages/FrasesPage';
import { LegacyImportPage } from './pages/LegacyImportPage';

export const App: React.FC = () => {
  return (
    <BrowserRouter>
      <div className="app-layout">
        <Navbar />
        <main className="main-content">
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/editoras" element={<EditorasPage />} />
            <Route path="/desejos" element={<DesejosPage />} />
            <Route path="/leituras" element={<LeiturasPage />} />
            <Route path="/frases" element={<FrasesPage />} />
            <Route path="/importacao-legado" element={<LegacyImportPage />} />
            <Route path="/configuracoes" element={<SettingsPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
};

export default App;
