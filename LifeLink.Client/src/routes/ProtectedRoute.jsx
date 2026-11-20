import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../features/authentication/context/AuthContext'
import LoadingSpinner from '../components/common/LoadingSpinner'

function ProtectedRoute({ allowedRoles }) {
    const { user, isAuthenticated, isLoading } = useAuth()

    if (isLoading) {
        return <LoadingSpinner />
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />
    }

    // Role authorization
    if (allowedRoles && !allowedRoles.includes(user?.role)) {
        return <Navigate to="/dashboard" replace />
    }

    return <Outlet />
}

export default ProtectedRoute