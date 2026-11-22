import { useState } from 'react'
import { useNotifications } from '../context/NotificationContext'
import './NotificationBell.css'
import { useNavigate } from 'react-router-dom'

const NotificationBell = () => {
    const [isOpen, setIsOpen] = useState(false)
    const navigate = useNavigate()

    const {
        notifications,
        unreadCount,
        markAsRead,
        markAllAsRead,
        removeNotification,
        clearNotifications,
    } = useNotifications()

    const handleNotificationClick = (notification) => {
        markAsRead(notification.id)
        setIsOpen(false)

        if (notification.bloodRequestId) {
            navigate('/dashboard', {
                state: {
                    highlightRequestId: notification.bloodRequestId,
                },
            })
        }
    }

    return (
        <div className="notification-container">
            <button
                type="button"
                className="notification-bell-button"
                onClick={() => setIsOpen((previous) => !previous)}
                aria-label={`Notifications. ${unreadCount} unread`}
                aria-expanded={isOpen}
            >
                <span className="notification-bell-icon">🔔</span>

                {unreadCount > 0 && (
                    <span className="notification-badge">
                        {unreadCount > 99 ? '99+' : unreadCount}
                    </span>
                )}
            </button>

            {isOpen && (
                <div className="notification-dropdown">
                    <div className="notification-header">
                        <div>
                            <h5>Notifications</h5>

                            <span className="notification-subtitle">
                                {unreadCount > 0
                                    ? `${unreadCount} unread`
                                    : 'All caught up'}
                            </span>
                        </div>

                        <button
                            type="button"
                            className="notification-close-button"
                            onClick={() => setIsOpen(false)}
                            aria-label="Close notifications"
                        >
                            ×
                        </button>
                    </div>

                    {notifications.length > 0 && (
                        <div className="notification-actions">
                            {unreadCount > 0 && (
                                <button
                                    type="button"
                                    onClick={markAllAsRead}
                                >
                                    Mark all as read
                                </button>
                            )}

                            <button
                                type="button"
                                onClick={clearNotifications}
                            >
                                Clear all
                            </button>
                        </div>
                    )}

                    <div className="notification-list">
                        {notifications.length === 0 ? (
                            <div className="notification-empty">
                                <div className="notification-empty-icon">
                                    🔔
                                </div>

                                <h6>No notifications yet</h6>

                                <p>
                                    We'll notify you when something
                                    important happens.
                                </p>
                            </div>
                        ) : (
                            notifications.map((notification) => (
                                <div
                                    key={notification.id}
                                    className={`notification-item ${notification.isRead
                                            ? 'read'
                                            : 'unread'
                                        }`}
                                >
                                    <button
                                        type="button"
                                        className="notification-content"
                                        onClick={() =>
                                            handleNotificationClick(
                                                notification
                                            )
                                        }
                                    >
                                        <div className="notification-item-icon">
                                            🩸
                                        </div>

                                        <div className="notification-text">
                                            <h6>
                                                {notification.title}
                                            </h6>

                                            <p>
                                                {notification.message}
                                            </p>

                                            <span className="notification-time">
                                                {new Date(
                                                    notification.receivedAt
                                                ).toLocaleTimeString(
                                                    [],
                                                    {
                                                        hour: '2-digit',
                                                        minute: '2-digit',
                                                    }
                                                )}
                                            </span>
                                        </div>

                                        {!notification.isRead && (
                                            <span className="unread-dot" />
                                        )}
                                    </button>

                                    <button
                                        type="button"
                                        className="notification-delete-button"
                                        onClick={() =>
                                            removeNotification(
                                                notification.id
                                            )
                                        }
                                        aria-label="Remove notification"
                                    >
                                        ×
                                    </button>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    )
}

export default NotificationBell