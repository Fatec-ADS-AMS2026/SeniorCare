import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ThemeProvider } from './contexts/ThemeContext.tsx';
import { AuthProvider } from './contexts/AuthContext.tsx';
import { RuntimeConfigProvider } from './contexts/RuntimeConfigContext.tsx';
import App from './App.tsx';
import './index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RuntimeConfigProvider>
      <ThemeProvider>
        <AuthProvider>
          <App />
        </AuthProvider>
      </ThemeProvider>
    </RuntimeConfigProvider>
  </StrictMode>
);
