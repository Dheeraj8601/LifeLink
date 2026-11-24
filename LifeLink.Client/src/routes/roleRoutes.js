
export const getUserRole = (user) => {
    return user?.role || user?.Role || ''
}

export const getDashboardPath = (user) => {
    const role = getUserRole(user)

    switch (role.toLowerCase()) {
        case 'admin':
            return '/admin/dashboard'

        case 'donor':
        case 'requester':
            return '/dashboard'

        default:
            return '/dashboard'
    }
}
