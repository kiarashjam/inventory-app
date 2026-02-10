import './i18n';
import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { ConfigProvider } from 'antd';
import { Toaster } from 'react-hot-toast';
import { store, persistor } from './redux/store';
import { antdTheme } from './config/antdTheme';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <ConfigProvider theme={antdTheme}>
          <App />
          <Toaster position="top-right" />
        </ConfigProvider>
      </PersistGate>
    </Provider>
  </React.StrictMode>
);
