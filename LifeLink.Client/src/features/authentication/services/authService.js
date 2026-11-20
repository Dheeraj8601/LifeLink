import apiClient from '../../../core/api/apiClient'

const authService = {
  login: async (credentials) => {
    const response = await apiClient.post('/Auth/login', credentials)
    return response.data
  },

  logout: async () => {
    const response = await apiClient.post('/Auth/logout')
    return response.data
  },

  getCurrentUser: async () => {
    const response = await apiClient.get('/Auth/me')
    return response.data
  },

  register: async (donorData) => {
    const response = await apiClient.post('/donors', donorData)
    return response.data
},
}

export default authService