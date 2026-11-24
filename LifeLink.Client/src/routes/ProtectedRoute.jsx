
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../features/authentication/context/AuthContext'
import LoadingSpinner from '../components/common/LoadingSpinner'
import { getDashboardPath, getUserRole } from './roleRoutes'

function ProtectedRoute({ allowedRoles }) {
    const { user, isAuthenticated, isLoading } = useAuth()

    if (isLoading) {
        return <LoadingSpinner />
    }

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />
    }

    const userRole = getUserRole(user)

    const isAllowed = allowedRoles?.some(
        role => role.toLowerCase() === userRole.toLowerCase()
    )

    // Role authorization
    if (allowedRoles && !isAllowed) {
        return (
            <Navigate
                to={getDashboardPath(user)}
                replace
            />
        )
    }

    return <Outlet />
}

export default ProtectedRoute
