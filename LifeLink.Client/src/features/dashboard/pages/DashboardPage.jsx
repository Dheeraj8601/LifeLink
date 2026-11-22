import Navbar from '../../../components/layout/Navbar'
import Footer from '../../../components/layout/Footer'
import { useAuth } from '../../authentication/context/AuthContext'
import DashboardCard from '../components/DashboardCard'
import { useEffect, useState } from 'react'
import donorService from '../services/donorService'
import './DashboardPage.css'
import bloodRequestService from '../../bloodRequests/services/bloodRequestService'
import RequesterDashboard from '../components/RequesterDashboard'
import DonorOpenRequests from '../components/DonorOpenRequests'
import MyResponses from '../components/MyResponses'
import notificationService from '../../../core/signalr/notificationService'
import { useLocation } from 'react-router-dom'

function DashboardPage() {
    const { user } = useAuth()
    const location = useLocation()

    // Request ID received when navigating from a notification
    const highlightRequestId =
        location.state?.highlightRequestId

    const userRole = user?.Role || user?.role
    const isDonor = userRole === 'Donor'
    const isRequester = userRole === 'Requester'

    const [donorProfile, setDonorProfile] = useState(null)
    const [isLoadingProfile, setIsLoadingProfile] = useState(true)
    const [profileError, setProfileError] = useState(null)

    const [bloodRequests, setBloodRequests] = useState([])
    const [isLoadingRequests, setIsLoadingRequests] = useState(true)
    const [requestsError, setRequestsError] = useState(null)

    const [isUpdatingAvailability, setIsUpdatingAvailability] = useState(false)
    const [availabilityError, setAvailabilityError] = useState(null)

    // Fetch donor profile when dashboard loads
    useEffect(() => {
        if (!isDonor) {
            setIsLoadingProfile(false)
            return
        }

        const fetchDonorProfile = async () => {
            try {
                const profile = await donorService.getMyProfile()
                setDonorProfile(profile)
            } catch (error) {
                setProfileError(
                    error.message || 'Failed to load donor profile.'
                )
            } finally {
                setIsLoadingProfile(false)
            }
        }

        fetchDonorProfile()
    }, [isDonor])

    // Fetch blood requests when the dashboard belongs to a requester
    useEffect(() => {
        if (!isRequester) {
            setIsLoadingRequests(false)
            return
        }

        const fetchBloodRequests = async () => {
            try {
                const requests =
                    await bloodRequestService.getMyBloodRequests()

                setBloodRequests(requests)
            } catch (error) {
                setRequestsError(
                    error.message || 'Failed to load your blood requests.'
                )
            } finally {
                setIsLoadingRequests(false)
            }
        }

        fetchBloodRequests()
    }, [isRequester])

    // Listen for real-time notifications for requester
    useEffect(() => {
        if (!isRequester) {
            return
        }

        const handleNewBloodRequestResponse = (notification) => {
            console.log(
                '🔔 New blood request response received:',
                notification
            )
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
    }, [isRequester])

    // Update donor availability
    const handleAvailabilityChange = async (newAvailability) => {
        if (
            !donorProfile ||
            donorProfile.isAvailable === newAvailability
        ) {
            return
        }

        setIsUpdatingAvailability(true)
        setAvailabilityError(null)

        try {
            await donorService.updateAvailability(newAvailability)

            setDonorProfile(previousProfile => ({
                ...previousProfile,
                isAvailable: newAvailability,
            }))
        } catch (error) {
            setAvailabilityError(
                error.message || 'Failed to update availability.'
            )
        } finally {
            setIsUpdatingAvailability(false)
        }
    }

    return (
        <>
            <Navbar />

            {isRequester ? (
                <RequesterDashboard
                    bloodRequests={bloodRequests}
                    isLoading={isLoadingRequests}
                    error={requestsError}
                    highlightRequestId={highlightRequestId}
                />
            ) : (
                <main className="dashboard-container container py-5">

                    {/* Dashboard Header */}
                    <div className="dashboard-header">
                        <div className="dashboard-welcome">
                            <h1>
                                Welcome, {user?.fullName || user?.FullName || 'Donor'}!
                            </h1>

                            <p>
                                Welcome to your LifeLink dashboard.
                            </p>
                        </div>

                        {/* Availability Toggle */}
                        <div className="availability-control">
                            <span className="availability-label">
                                Donor Status:
                            </span>

                            <div
                                className={`availability-switch ${
                                    donorProfile?.isAvailable
                                        ? 'status-available'
                                        : 'status-unavailable'
                                }`}
                            >
                                <button
                                    type="button"
                                    className={`availability-option ${
                                        donorProfile?.isAvailable
                                            ? 'active'
                                            : ''
                                    }`}
                                    onClick={() =>
                                        handleAvailabilityChange(true)
                                    }
                                    disabled={
                                        isLoadingProfile ||
                                        isUpdatingAvailability ||
                                        !donorProfile
                                    }
                                    aria-pressed={donorProfile?.isAvailable}
                                >
                                    <span>Available</span>

                                    <span className="status-indicator available-indicator" />
                                </button>

                                <button
                                    type="button"
                                    className={`availability-option ${
                                        donorProfile &&
                                        !donorProfile.isAvailable
                                            ? 'active'
                                            : ''
                                    }`}
                                    onClick={() =>
                                        handleAvailabilityChange(false)
                                    }
                                    disabled={
                                        isLoadingProfile ||
                                        isUpdatingAvailability ||
                                        !donorProfile
                                    }
                                    aria-pressed={
                                        donorProfile
                                            ? !donorProfile.isAvailable
                                            : false
                                    }
                                >
                                    <span>Unavailable</span>

                                    <span className="status-indicator unavailable-indicator" />
                                </button>
                            </div>
                        </div>
                    </div>

                    {/* Loading State */}
                    {isLoadingProfile && (
                        <div className="alert alert-info">
                            Loading your donor profile...
                        </div>
                    )}

                    {/* Profile Error */}
                    {profileError && (
                        <div className="alert alert-danger" role="alert">
                            {profileError}
                        </div>
                    )}

                    {/* Availability Update Error */}
                    {availabilityError && (
                        <div className="alert alert-danger" role="alert">
                            {availabilityError}
                        </div>
                    )}

                    {/* Dashboard Cards */}
                    {!isLoadingProfile && donorProfile && (
                        <div className="row g-4 dashboard-cards">

                            {/* Blood Group */}
                            <div className="col-md-4">
                                <DashboardCard
                                    title="Blood Group"
                                    value={donorProfile.bloodGroup || '--'}
                                    icon="🩸"
                                    description="Your registered blood group"
                                />
                            </div>

                            {/* Location */}
                            <div className="col-md-4">
                                <DashboardCard
                                    title="Location"
                                    value={
                                        donorProfile.cityName
                                            ? `${donorProfile.cityName}, ${donorProfile.cityState}`
                                            : '--'
                                    }
                                    icon="📍"
                                    description="Your registered city"
                                />
                            </div>

                            {/* Availability */}
                            <div className="col-md-4">
                                <DashboardCard
                                    title="Availability"
                                    value={
                                        donorProfile.isAvailable
                                            ? 'Available'
                                            : 'Unavailable'
                                    }
                                    icon={
                                        donorProfile.isAvailable
                                            ? '🟢'
                                            : '🔴'
                                    }
                                    description="Donor availability status"
                                />
                            </div>

                        </div>
                    )}

                    {/* Open Blood Requests */}
                    <DonorOpenRequests />

                    {/* Donor Response History */}
                    <section className="mt-5">
                        <MyResponses />
                    </section>

                </main>
            )}

            <Footer />
        </>
    )
}

export default DashboardPage