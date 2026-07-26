import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import './index.css'
import i18n from './i18n/i18n.ts'
import App from './App.tsx'
import { store } from './store/store.ts'

// index.html sabit lang="tr" ile başlar — localStorage'daki gerçek tercih 'de' olabilir,
// ekran okuyucular için <html lang> ilk render'dan itibaren doğru olmalı.
document.documentElement.lang = i18n.language

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <App />
    </Provider>
  </StrictMode>,
)
