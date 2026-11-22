import React from 'react'
import ReactDOM from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { NotificationProvider } from './features/notifications/context/NotificationContext'
import { AuthProvider } from './features/authentication/context/AuthContext'

import 'bootstrap/dist/css/bootstrap.min.css'
import './index.css'

import App from './App.jsx'

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <BrowserRouter>
            <AuthProvider>
                <NotificationProvider>
                    <App />
                </NotificationProvider>
            </AuthProvider>
        </BrowserRouter>
    </React.StrictMode>
)