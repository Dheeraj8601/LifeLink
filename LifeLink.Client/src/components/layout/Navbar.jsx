import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../../features/authentication/context/AuthContext'
import NotificationBell from '../../features/notifications/components/NotificationBell'

function Navbar() {
    const { user, isAuthenticated, isLoading, logout } = useAuth()
    const navigate = useNavigate()

    const handleLogout = async () => {
        await logout()
        navigate('/login')
    }

    return (
        <nav className="navbar navbar-expand-lg navbar-dark bg-danger">
            <div className="container">
                <Link className="navbar-brand fw-bold" to="/">
                    ❤️ LifeLink
                </Link>

                <div className="ms-auto d-flex align-items-center gap-2">
                    {!isLoading && !isAuthenticated && (
                        <>
                            <Link
                                className="btn btn-outline-light"
                                to="/login"
                            >
                                Login
                            </Link>

                            <Link
                                className="btn btn-light text-danger fw-semibold"
                                to="/register"
                            >
                                Register
                            </Link>
                        </>
                    )}

                    {!isLoading && isAuthenticated && (
                        <>
                            <span className="text-white">
                                Welcome,{' '}
                                {user?.fullName ||
                                    user?.FullName ||
                                    user?.email ||
                                    user?.Email}
                            </span>

                            <NotificationBell />

                            <button
                                className="btn btn-light text-danger fw-semibold"
                                onClick={handleLogout}
                            >
                                Logout
                            </button>
                        </>
                    )}
                </div>
            </div>
        </nav>
    )
}

export default Navbar