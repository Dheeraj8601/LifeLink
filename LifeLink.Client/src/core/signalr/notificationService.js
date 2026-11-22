import {
    HubConnectionBuilder,
    LogLevel,
} from '@microsoft/signalr'

const HUB_URL = 'https://localhost:7269/hubs/notifications'

let connection = null
let connectionPromise = null

// Store event handlers even if SignalR is not connected yet
const eventHandlers = new Map()

const notificationService = {
    connect: async () => {
        if (connection?.state === 'Connected') {
            return connection
        }

        // Prevent multiple simultaneous connection attempts
        if (connectionPromise) {
            return connectionPromise
        }

        connection = new HubConnectionBuilder()
            .withUrl(HUB_URL, {
                withCredentials: true,
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build()

        // Register previously stored event handlers
        eventHandlers.forEach((callbacks, eventName) => {
            callbacks.forEach(callback => {
                connection.on(eventName, callback)
            })
        })

        connection.onreconnecting((error) => {
            console.warn('SignalR reconnecting...', error)
        })

        connection.onreconnected((connectionId) => {
            console.log(
                'SignalR reconnected. Connection ID:',
                connectionId
            )
        })

        connection.onclose((error) => {
            console.warn('SignalR connection closed.', error)
        })

        connectionPromise = connection.start()
            .then(() => {
                console.log('SignalR connected successfully.')
                return connection
            })
            .catch((error) => {
                console.error(
                    'SignalR connection failed:',
                    error
                )

                connection = null
                connectionPromise = null

                throw error
            })

        return connectionPromise
    },

    disconnect: async () => {
        if (connection) {
            await connection.stop()

            connection = null
            connectionPromise = null

            console.log('SignalR disconnected.')
        }
    },

    on: (eventName, callback) => {
        if (!eventHandlers.has(eventName)) {
            eventHandlers.set(eventName, new Set())
        }

        eventHandlers.get(eventName).add(callback)

        // Register immediately if connection already exists
        if (connection) {
            connection.on(eventName, callback)
        }
    },

    off: (eventName, callback) => {
        const callbacks = eventHandlers.get(eventName)

        if (callbacks) {
            callbacks.delete(callback)

            if (callbacks.size === 0) {
                eventHandlers.delete(eventName)
            }
        }

        if (connection) {
            connection.off(eventName, callback)
        }
    },

    getConnection: () => connection,
}

export default notificationService