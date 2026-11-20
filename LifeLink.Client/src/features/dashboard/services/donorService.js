import apiClient from '../../../core/api/apiClient'

const donorService = {
    getMyProfile: async () => {
        const response = await apiClient.get('/donors/me')
        return response.data
    },

    updateAvailability: async (isAvailable) => {
        const response = await apiClient.patch(
            '/donors/me/availability',
            { isAvailable }
        )

        return response.data
    },
}

export default donorService