
import { Routes, Route } from 'react-router-dom'

import DashboardPage from '../features/dashboard/pages/DashboardPage'
import AdminDashboard from '../features/admin/pages/AdminDashboard'
import HomePage from '../features/home/pages/HomePage'
import LoginPage from '../features/authentication/pages/LoginPage'
import RegisterPage from '../features/authentication/pages/RegisterPage'
import CreateBloodRequestPage from '../features/bloodRequests/pages/CreateBloodRequestPage'

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

            {/* Donor and Requester dashboard */}
            <Route
                element={
                    <ProtectedRoute
                        allowedRoles={['Donor', 'Requester']}
                    />
                }
            >
                <Route
                    path="/dashboard"
                    element={<DashboardPage />}
                />
            </Route>

            {/* Admin-only dashboard */}
            <Route
                element={
                    <ProtectedRoute
                        allowedRoles={['Admin']}
                    />
                }
            >
                <Route
                    path="/admin/dashboard"
                    element={<AdminDashboard />}
                />
            </Route>

            {/* Requester-only routes */}
            <Route
                element={
                    <ProtectedRoute
                        allowedRoles={['Requester']}
                    />
                }
            >
                <Route
                    path="/blood-requests/create"
                    element={<CreateBloodRequestPage />}
                />
            </Route>
        </Routes>
    )
}

export default AppRoutes
