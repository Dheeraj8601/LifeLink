import apiClient from '../api/apiClient'

const cityService = {
    getAllCities: async () => {
        const response = await apiClient.get('/cities')
        return response.data
    },
}

export default cityService