import {
    createContext,
    useContext,
    useEffect,
    useState,
} from 'react'

import authService from '../services/authService'
import notificationService from '../../../core/signalr/notificationService'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null)
    const [isAuthenticated, setIsAuthenticated] = useState(false)
    const [isLoading, setIsLoading] = useState(true)

    // Handle unauthorized API responses globally
    useEffect(() => {
        const handleUnauthorized = () => {
            setUser(null)
            setIsAuthenticated(false)
        }

        window.addEventListener(
            'auth:unauthorized',
            handleUnauthorized
        )

        return () => {
            window.removeEventListener(
                'auth:unauthorized',
                handleUnauthorized
            )
        }
    }, [])

    // Check authentication when the application starts
    useEffect(() => {
        const checkAuthentication = async () => {
            try {
                const currentUser = await authService.getCurrentUser()

                setUser(currentUser)
                setIsAuthenticated(true)
            } catch {
                setUser(null)
                setIsAuthenticated(false)
            } finally {
                setIsLoading(false)
            }
        }

        checkAuthentication()
    }, [])

    // Manage SignalR connection based on authentication
    useEffect(() => {
        if (!isAuthenticated) {
            notificationService.disconnect()
            return
        }

        const connectSignalR = async () => {
            try {
                await notificationService.connect()
            } catch (error) {
                console.error(
                    'Failed to establish notification connection:',
                    error
                )
            }
        }

        connectSignalR()

        return () => {
            notificationService.disconnect()
        }
    }, [isAuthenticated])

    // Login
    const login = async (credentials) => {
        const result = await authService.login(credentials)

        setUser(result.user)
        setIsAuthenticated(true)

        return result
    }

    // Logout
    const logout = async () => {
        try {
            await authService.logout()
        } finally {
            setUser(null)
            setIsAuthenticated(false)
        }
    }

    const value = {
        user,
        isAuthenticated,
        isLoading,
        login,
        logout,
    }

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    )
}

// Custom hook to access authentication context
export function useAuth() {
    const context = useContext(AuthContext)

    if (!context) {
        throw new Error(
            'useAuth must be used within an AuthProvider'
        )
    }

    return context
}