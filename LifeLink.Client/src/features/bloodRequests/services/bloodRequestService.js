
import apiClient from '../../../core/api/apiClient'

const bloodRequestService = {
    // Create a new blood request
    createBloodRequest: async (requestData) => {
        const response = await apiClient.post(
            '/blood-requests',
            requestData
        )

        return response.data
    },

    // Get all blood requests created by the logged-in requester
    getMyBloodRequests: async () => {
        const response = await apiClient.get(
            '/blood-requests/my-requests'
        )

        return response.data
    },

    // Get nearby eligible donors for a blood request
    getNearbyDonors: async (
        requestId,
        radiusKm = 20,
        pageNumber = 1,
        pageSize = 10
    ) => {
        const response = await apiClient.get(
            `/blood-requests/${requestId}/nearby-donors`,
            {
                params: {
                    radiusKm,
                    pageNumber,
                    pageSize,
                },
            }
        )

        return response.data
    },

    // Get donor responses for a specific blood request
    getResponsesForRequest: async (requestId) => {
        const response = await apiClient.get(
            `/blood-requests/${requestId}/responses`
        )

        return response.data
    },

    // Accept or reject a donor response
    updateResponseStatus: async (responseId, status) => {
        const response = await apiClient.put(
            `/blood-requests/responses/${responseId}/status`,
            {
                status,
            }
        )

        return response.data
    },
}

export default bloodRequestService
