
import apiClient from "../../../core/api/apiClient";

const adminService = {
    getPendingDonors: async () => {
        const response = await apiClient.get("/admin/donors/pending");
        return response.data;
    },

    verifyDonor: async (donorId) => {
        const response = await apiClient.put(
            `/admin/donors/${donorId}/verify`
        );
        return response.data;
    },

    rejectDonor: async (donorId, rejectionReason) => {
        const response = await apiClient.put(
            `/admin/donors/${donorId}/reject`,
            { rejectionReason }
        );
        return response.data;
    }
};

export default adminService;
