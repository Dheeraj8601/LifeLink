import {
    createContext,
    useCallback,
    useContext,
    useEffect,
    useState,
} from 'react'

import { useAuth } from '../../authentication/context/AuthContext'
import notificationService from '../../../core/signalr/notificationService'

const NotificationContext = createContext(null)

export const NotificationProvider = ({ children }) => {
    const [notifications, setNotifications] = useState([])
    const { isAuthenticated } = useAuth()

    // Add a notification to the centralized state
    const addNotification = useCallback((notification) => {
        const newNotification = {
            id: crypto.randomUUID(),
            ...notification,
            isRead: false,
            receivedAt: new Date(),
        }

        setNotifications((previous) => [
            newNotification,
            ...previous,
        ])
    }, [])

    // Mark one notification as read
    const markAsRead = useCallback((notificationId) => {
        setNotifications((previous) =>
            previous.map((notification) =>
                notification.id === notificationId
                    ? { ...notification, isRead: true }
                    : notification
            )
        )
    }, [])

    // Mark all notifications as read
    const markAllAsRead = useCallback(() => {
        setNotifications((previous) =>
            previous.map((notification) => ({
                ...notification,
                isRead: true,
            }))
        )
    }, [])

    // Remove a notification
    const removeNotification = useCallback((notificationId) => {
        setNotifications((previous) =>
            previous.filter(
                (notification) =>
                    notification.id !== notificationId
            )
        )
    }, [])

    // Clear all notifications
    const clearNotifications = useCallback(() => {
        setNotifications([])
    }, [])

    // Listen for real-time SignalR notifications
    useEffect(() => {
        if (!isAuthenticated) {
            return
        }

        const handleNewBloodRequestResponse = (payload) => {
            console.log(
                '🔔 SignalR notification received:',
                payload
            )

            addNotification({
                type: 'BloodRequestResponse',
                title: 'New donor response',
                message:
                    payload.message ??
                    'A donor has responded to your blood request.',
                bloodRequestId: payload.bloodRequestId ??
                    payload.BloodRequestId,
                responseId: payload.responseId ??
                    payload.ResponseId,
                donorUserId: payload.donorUserId ??
                    payload.DonorUserId,
            })
        }

        notificationService.on(
            'NewBloodRequestResponse',
            handleNewBloodRequestResponse
        )

        return () => {
            notificationService.off(
                'NewBloodRequestResponse',
                handleNewBloodRequestResponse
            )
        }
    }, [isAuthenticated, addNotification])

    // Calculate unread notifications
    const unreadCount = notifications.filter(
        (notification) => !notification.isRead
    ).length

    const value = {
        notifications,
        unreadCount,
        addNotification,
        markAsRead,
        markAllAsRead,
        removeNotification,
        clearNotifications,
    }

    return (
        <NotificationContext.Provider value={value}>
            {children}
        </NotificationContext.Provider>
    )
}

export const useNotifications = () => {
    const context = useContext(NotificationContext)

    if (!context) {
        throw new Error(
            'useNotifications must be used inside NotificationProvider'
        )
    }

    return context
}