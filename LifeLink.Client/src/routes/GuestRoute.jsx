
import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../features/authentication/context/AuthContext'
import LoadingSpinner from '../components/common/LoadingSpinner'
import { getDashboardPath } from './roleRoutes'

function GuestRoute() {
    const { user, isAuthenticated, isLoading } = useAuth()

    if (isLoading) {
        return <LoadingSpinner />
    }

    if (isAuthenticated) {
        return (
            <Navigate
                to={getDashboardPath(user)}
                replace
            />
        )
    }

    return <Outlet />
}

export default GuestRoute
