
import { useState } from 'react'
import { Link, useNavigate, useLocation } from 'react-router-dom'
import FormInput from '../../../components/common/FormInput'
import AuthLayout from './AuthLayout'
import { useAuth } from '../context/AuthContext'

function LoginForm() {
    const { login } = useAuth()
    const navigate = useNavigate()
    const location = useLocation()
    const successMessage = location.state?.message
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [error, setError] = useState('')
    const [isLoading, setIsLoading] = useState(false)



    const handleSubmit = async (e) => {
        e.preventDefault()

        setError('')
        setIsLoading(true)

        try {
            const result = await login({
                email: email.trim(),
                password,
            })

            console.log('Login successful:', result.user)

            navigate('/dashboard')
        } catch (error) {
            setError(error.message || 'Login failed. Please try again.')
        } finally {
            setIsLoading(false)
        }
    }

    return (
        <AuthLayout
            title="Welcome Back"
            subtitle="Login to your LifeLink account"
        >
            <form onSubmit={handleSubmit}>
                {error && (
                    <div className="alert alert-danger">
                        {error}
                    </div>
                )}
                {successMessage && (
                    <div className="alert alert-success" role="alert">
                        {successMessage}
                    </div>
                )}

                <FormInput
                    label="Email Address"
                    type="email"
                    name="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    placeholder="Enter your email"
                    required
                />

                <FormInput
                    label="Password"
                    type="password"
                    name="password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    placeholder="Enter your password"
                    required
                />

                <button
                    type="submit"
                    className="btn btn-danger w-100"
                    disabled={isLoading}
                >
                    {isLoading ? 'Logging in...' : 'Login'}
                </button>
            </form>

            <div className="text-center mt-4">
                <p className="mb-0">
                    Don't have an account?{' '}
                    <Link
                        to="/register"
                        className="text-danger fw-semibold text-decoration-none"
                    >
                        Register
                    </Link>
                </p>
            </div>
        </AuthLayout>
    )
}

export default LoginForm
