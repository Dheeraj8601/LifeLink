import apiClient from '../../../core/api/apiClient'

const bloodRequestService = {
    createBloodRequest: async (requestData) => {
        const response = await apiClient.post(
            '/blood-requests',
            requestData
        )

        return response.data
    },
}

export default bloodRequestService