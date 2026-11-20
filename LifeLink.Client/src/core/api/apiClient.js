
import axios from 'axios'

const apiClient = axios.create({
    baseURL: 'https://localhost:7269/api',
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
    },
})

apiClient.interceptors.response.use(
    (response) => response,

    (error) => {
        const status = error.response?.status
        const requestUrl = error.config?.url || ''

        // Don't treat invalid login credentials as an expired session.
        if (
            status === 401 &&
            !requestUrl.toLowerCase().includes('/auth/login')
        ) {
            window.dispatchEvent(
                new Event('auth:unauthorized')
            )
        }

        const apiError = {
            status: status ?? null,
            message:
                error.response?.data?.message ||
                error.message ||
                'An unexpected error occurred.',
            data: error.response?.data ?? null,
        }

        return Promise.reject(apiError)
    }
)

export default apiClient
