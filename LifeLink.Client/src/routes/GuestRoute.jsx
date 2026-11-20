
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../features/authentication/context/AuthContext'
import LoadingSpinner from '../components/common/LoadingSpinner'

function GuestRoute() {
    const { isAuthenticated, isLoading } = useAuth()

    if (isLoading) {
        return <LoadingSpinner />
    }

    if (isAuthenticated) {
        return <Navigate to="/" replace />
    }

    return <Outlet />
}

export default GuestRoute
