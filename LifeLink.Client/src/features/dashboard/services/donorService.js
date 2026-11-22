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

    getOpenBloodRequests: async (pageNumber = 1, pageSize = 10) => {
        const response = await apiClient.get(
            '/blood-requests/open',
            {
                params: {
                    pageNumber,
                    pageSize,
                },
            }
        )

        return response.data
    },

    submitResponse: async (bloodRequestId) => {
        const response = await apiClient.post(
            `/blood-requests/${bloodRequestId}/responses`
        )

        return response.data
    },

    getMyResponses: async () => {
        const response = await apiClient.get('/blood-requests/my-responses')
        return response.data
    },
}

export default donorService