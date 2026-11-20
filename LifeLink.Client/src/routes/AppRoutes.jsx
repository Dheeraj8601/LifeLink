import { Routes, Route } from 'react-router-dom'
import DashboardPage from '../features/dashboard/pages/DashboardPage'
import HomePage from '../features/home/pages/HomePage'
import LoginPage from '../features/authentication/pages/LoginPage'
import RegisterPage from '../features/authentication/pages/RegisterPage'

import ProtectedRoute from './ProtectedRoute'
import GuestRoute from './GuestRoute'

function AppRoutes() {
    return (
        <Routes>
            {/* Public route */}
            <Route path="/" element={<HomePage />} />

            {/* Guest-only routes */}
            <Route element={<GuestRoute />}>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
            </Route>

            {/* Protected routes */}
            <Route element={<ProtectedRoute />}>
                <Route path="/dashboard" element={<DashboardPage />} />
            </Route>
        </Routes>
    )
}

export default AppRoutes