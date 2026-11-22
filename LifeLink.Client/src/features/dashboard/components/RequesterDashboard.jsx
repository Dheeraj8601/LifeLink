import { Link } from 'react-router-dom'
import { useEffect, useRef, useState } from 'react'
import bloodRequestService from '../../bloodRequests/services/bloodRequestService'

function RequesterDashboard({
    bloodRequests,
    isLoading,
    error,
    highlightRequestId,
}) {
    const highlightedRequestRef = useRef(null)

    const [nearbyDonors, setNearbyDonors] = useState({})
    const [loadingDonors, setLoadingDonors] = useState(null)
    const [donorErrors, setDonorErrors] = useState({})

    const [responses, setResponses] = useState({})
    const [loadingResponses, setLoadingResponses] = useState(null)
    const [responseErrors, setResponseErrors] = useState({})
    const [visibleResponses, setVisibleResponses] = useState({})

    const [updatingResponseId, setUpdatingResponseId] = useState(null)
    const [responseActionErrors, setResponseActionErrors] = useState({})
    const [responseActionMessages, setResponseActionMessages] = useState({})

    const totalRequests = bloodRequests.length

    const openRequests = bloodRequests.filter(
        request => request.status?.toLowerCase() === 'open'
    ).length

    const fulfilledRequests = bloodRequests.filter(
        request => request.status?.toLowerCase() === 'fulfilled'
    ).length

    // Scroll to the request selected from a notification
    useEffect(() => {
        if (
            isLoading ||
            !highlightRequestId ||
            !highlightedRequestRef.current
        ) {
            return
        }

        highlightedRequestRef.current.scrollIntoView({
            behavior: 'smooth',
            block: 'center',
        })
    }, [isLoading, highlightRequestId, bloodRequests])

    const getStatusClass = (status) => {
        switch (status?.toLowerCase()) {
            case 'open':
                return 'request-status-open'
            case 'fulfilled':
                return 'request-status-fulfilled'
            case 'cancelled':
                return 'request-status-cancelled'
            default:
                return 'request-status-default'
        }
    }

    const getUrgencyClass = (urgency) => {
        switch (urgency?.toLowerCase()) {
            case 'critical':
                return 'request-urgency-critical'
            case 'urgent':
                return 'request-urgency-urgent'
            default:
                return 'request-urgency-normal'
        }
    }

    const loadNearbyDonors = async (requestId) => {
        setLoadingDonors(requestId)

        setDonorErrors(prev => ({
            ...prev,
            [requestId]: null,
        }))

        try {
            const result = await bloodRequestService.getNearbyDonors(
                requestId
            )

            setNearbyDonors(prev => ({
                ...prev,
                [requestId]: result,
            }))
        } catch (error) {
            setDonorErrors(prev => ({
                ...prev,
                [requestId]:
                    error.response?.data?.message ||
                    'Failed to load nearby donors.',
            }))
        } finally {
            setLoadingDonors(null)
        }
    }

    const loadResponses = async (requestId) => {
        // Toggle visibility if already open
        if (visibleResponses[requestId]) {
            setVisibleResponses(prev => ({
                ...prev,
                [requestId]: false,
            }))
            return
        }

        setVisibleResponses(prev => ({
            ...prev,
            [requestId]: true,
        }))

        setLoadingResponses(requestId)

        setResponseErrors(prev => ({
            ...prev,
            [requestId]: null,
        }))

        try {
            const result = await bloodRequestService.getResponsesForRequest(
                requestId
            )

            setResponses(prev => ({
                ...prev,
                [requestId]: result,
            }))
        } catch (error) {
            setResponseErrors(prev => ({
                ...prev,
                [requestId]:
                    error.response?.data?.message ||
                    'Failed to load donor responses.',
            }))
        } finally {
            setLoadingResponses(null)
        }
    }

    // Accept or reject a donor response
    const handleResponseStatusUpdate = async (
        requestId,
        responseId,
        newStatus
    ) => {
        setUpdatingResponseId(responseId)

        setResponseActionErrors(prev => ({
            ...prev,
            [responseId]: null,
        }))

        setResponseActionMessages(prev => ({
            ...prev,
            [responseId]: null,
        }))

        try {
            await bloodRequestService.updateResponseStatus(
                responseId,
                newStatus
            )

            // Reload responses to get the latest server state
            const updatedResponses =
                await bloodRequestService.getResponsesForRequest(
                    requestId
                )

            setResponses(prev => ({
                ...prev,
                [requestId]: updatedResponses,
            }))

            setResponseActionMessages(prev => ({
                ...prev,
                [responseId]:
                    `Donor response ${newStatus.toLowerCase()} successfully.`,
            }))
        } catch (error) {
            setResponseActionErrors(prev => ({
                ...prev,
                [responseId]:
                    error.response?.data?.message ||
                    `Failed to ${newStatus.toLowerCase()} donor response.`,
            }))
        } finally {
            setUpdatingResponseId(null)
        }
    }

    const getResponseStatus = (status) => {
        if (typeof status === 'number') {
            switch (status) {
                case 0:
                    return 'Pending'
                case 1:
                    return 'Accepted'
                case 2:
                    return 'Rejected'
                case 3:
                    return 'Withdrawn'
                default:
                    return 'Unknown'
            }
        }

        return String(status ?? 'Unknown')
    }

    return (
        <main className="requester-dashboard container py-5">

            {/* Header */}
            <div className="requester-dashboard-header">
                <h1>My Blood Requests</h1>

                <p>
                    Track your blood requests and find donors.
                </p>
            </div>

            {/* Loading */}
            {isLoading && (
                <div className="alert alert-info">
                    Loading your blood requests...
                </div>
            )}

            {/* Error */}
            {error && (
                <div className="alert alert-danger" role="alert">
                    {error}
                </div>
            )}

            {!isLoading && !error && (
                <>
                    {/* Summary */}
                    <div className="row g-4 requester-summary">

                        <div className="col-md-4">
                            <div className="requester-summary-card card">
                                <div className="card-body">
                                    <p className="requester-summary-label">
                                        Total Requests
                                    </p>

                                    <h3 className="requester-summary-value">
                                        {totalRequests}
                                    </h3>
                                </div>
                            </div>
                        </div>

                        <div className="col-md-4">
                            <div className="requester-summary-card card">
                                <div className="card-body">
                                    <p className="requester-summary-label">
                                        Open Requests
                                    </p>

                                    <h3 className="requester-summary-value">
                                        {openRequests}
                                    </h3>
                                </div>
                            </div>
                        </div>

                        <div className="col-md-4">
                            <div className="requester-summary-card card">
                                <div className="card-body">
                                    <p className="requester-summary-label">
                                        Fulfilled Requests
                                    </p>

                                    <h3 className="requester-summary-value">
                                        {fulfilledRequests}
                                    </h3>
                                </div>
                            </div>
                        </div>

                    </div>

                    {/* Create Request Button */}
                    <div className="d-flex justify-content-between align-items-center mb-4">
                        <h3 className="fw-bold mb-0">
                            Your Requests
                        </h3>

                        <Link
                            to="/blood-requests/create"
                            className="btn btn-danger"
                        >
                            + New Blood Request
                        </Link>
                    </div>

                    {/* Empty State */}
                    {bloodRequests.length === 0 && (
                        <div className="card shadow-sm border-0">
                            <div className="card-body text-center py-5">
                                <h4>No blood requests yet</h4>

                                <p className="text-muted">
                                    Create your first blood request to find
                                    eligible donors.
                                </p>

                                <Link
                                    to="/blood-requests/create"
                                    className="btn btn-danger mt-2"
                                >
                                    Create Blood Request
                                </Link>
                            </div>
                        </div>
                    )}

                    {/* Request Cards */}
                    {bloodRequests.length > 0 && (
                        <div className="row g-4">
                            {bloodRequests.map(request => {
                                const isHighlighted =
                                    String(request.id) ===
                                    String(highlightRequestId)

                                return (
                                    <div
                                        className="col-12 col-md-6 col-lg-4"
                                        key={request.id}
                                        ref={
                                            isHighlighted
                                                ? highlightedRequestRef
                                                : null
                                        }
                                    >
                                        <div
                                            className={`requester-request-card card ${
                                                isHighlighted
                                                    ? 'request-card-highlight'
                                                    : ''
                                            }`}
                                        >
                                            <div className="card-body">

                                                {/* Notification Highlight */}
                                                {isHighlighted && (
                                                    <div className="request-highlight-message">
                                                        🔔 New donor response
                                                        for this request
                                                    </div>
                                                )}

                                                {/* Blood Group */}
                                                <div className="d-flex justify-content-between align-items-start mb-3">
                                                    <h3 className="fw-bold mb-0">
                                                        {request.bloodGroup}
                                                    </h3>

                                                    <span
                                                        className={`request-status-badge ${getStatusClass(request.status)}`}
                                                    >
                                                        {request.status}
                                                    </span>
                                                </div>

                                                {/* Hospital */}
                                                <p className="mb-2">
                                                    <strong>Hospital:</strong>{' '}
                                                    {request.hospitalName}
                                                </p>

                                                {/* Units */}
                                                <p className="mb-2">
                                                    <strong>Units Required:</strong>{' '}
                                                    {request.unitsRequired}
                                                </p>

                                                {/* Urgency */}
                                                <p className="mb-2">
                                                    <strong>Urgency:</strong>{' '}
                                                    <span
                                                        className={`request-urgency-badge ${getUrgencyClass(request.urgency)}`}
                                                    >
                                                        {request.urgency}
                                                    </span>
                                                </p>

                                                {/* Hospital Address */}
                                                {request.hospitalAddress && (
                                                    <p className="mb-2">
                                                        <strong>Address:</strong>{' '}
                                                        {request.hospitalAddress}
                                                    </p>
                                                )}

                                                {/* Created Date */}
                                                <p className="text-muted small mt-3 mb-0">
                                                    Created:{' '}
                                                    {new Date(
                                                        request.createdAt
                                                    ).toLocaleDateString()}
                                                </p>

                                                {/* View Donor Responses */}
                                                <button
                                                    className="btn btn-outline-dark w-100 mt-3"
                                                    onClick={() =>
                                                        loadResponses(request.id)
                                                    }
                                                    disabled={
                                                        loadingResponses ===
                                                        request.id
                                                    }
                                                >
                                                    {loadingResponses ===
                                                    request.id
                                                        ? 'Loading Responses...'
                                                        : visibleResponses[
                                                            request.id
                                                        ]
                                                            ? 'Hide Responses'
                                                            : 'View Responses'}
                                                </button>

                                                {/* Donor Responses */}
                                                {visibleResponses[
                                                    request.id
                                                ] && (
                                                    <div className="donor-responses-section mt-4">

                                                        <h5 className="fw-bold mb-3">
                                                            Donor Responses
                                                        </h5>

                                                        {/* Loading */}
                                                        {loadingResponses ===
                                                            request.id && (
                                                            <div className="alert alert-info">
                                                                Loading donor responses...
                                                            </div>
                                                        )}

                                                        {/* Error */}
                                                        {responseErrors[
                                                            request.id
                                                        ] && (
                                                            <div className="alert alert-danger">
                                                                {
                                                                    responseErrors[
                                                                        request.id
                                                                    ]
                                                                }
                                                            </div>
                                                        )}

                                                        {/* No Responses */}
                                                        {responses[
                                                            request.id
                                                        ]?.length === 0 &&
                                                            loadingResponses !==
                                                                request.id &&
                                                            !responseErrors[
                                                                request.id
                                                            ] && (
                                                                <div className="alert alert-warning">
                                                                    No donors have responded to this request yet.
                                                                </div>
                                                            )}

                                                        {/* Response List */}
                                                        {responses[
                                                            request.id
                                                        ]?.length > 0 && (
                                                            <div className="d-flex flex-column gap-2">

                                                                {responses[
                                                                    request.id
                                                                ].map(response => (
                                                                    <div
                                                                        className="donor-response-card"
                                                                        key={response.id}
                                                                    >
                                                                        <div className="d-flex justify-content-between align-items-center">

                                                                            <div>
                                                                                <strong>
                                                                                    Donor #{response.donorId}
                                                                                </strong>

                                                                                <div className="text-muted small">
                                                                                    Responded:{' '}
                                                                                    {new Date(
                                                                                        response.respondedAt
                                                                                    ).toLocaleDateString()}
                                                                                </div>
                                                                            </div>

                                                                            <span className="badge bg-secondary">
                                                                                {getResponseStatus(
                                                                                    response.status
                                                                                )}
                                                                            </span>

                                                                        </div>

                                                                        {/* Response Action Messages */}
                                                                        {responseActionMessages[
                                                                            response.id
                                                                        ] && (
                                                                            <div className="alert alert-success mt-2 mb-0">
                                                                                {
                                                                                    responseActionMessages[
                                                                                        response.id
                                                                                    ]
                                                                                }
                                                                            </div>
                                                                        )}

                                                                        {responseActionErrors[
                                                                            response.id
                                                                        ] && (
                                                                            <div className="alert alert-danger mt-2 mb-0">
                                                                                {
                                                                                    responseActionErrors[
                                                                                        response.id
                                                                                    ]
                                                                                }
                                                                            </div>
                                                                        )}

                                                                        {/* Accept / Reject Pending Responses */}
                                                                        {getResponseStatus(
                                                                            response.status
                                                                        ).toLowerCase() ===
                                                                            'pending' && (
                                                                            <div className="row g-2 mt-2">
                                                                                <div className="col-6">
                                                                                    <button
                                                                                        className="btn btn-success w-100"
                                                                                        onClick={() =>
                                                                                            handleResponseStatusUpdate(
                                                                                                request.id,
                                                                                                response.id,
                                                                                                'Accepted'
                                                                                            )
                                                                                        }
                                                                                        disabled={
                                                                                            updatingResponseId ===
                                                                                            response.id
                                                                                        }
                                                                                    >
                                                                                        {updatingResponseId ===
                                                                                        response.id
                                                                                            ? 'Updating...'
                                                                                            : 'Accept'}
                                                                                    </button>
                                                                                </div>

                                                                                <div className="col-6">
                                                                                    <button
                                                                                        className="btn btn-outline-danger w-100"
                                                                                        onClick={() =>
                                                                                            handleResponseStatusUpdate(
                                                                                                request.id,
                                                                                                response.id,
                                                                                                'Rejected'
                                                                                            )
                                                                                        }
                                                                                        disabled={
                                                                                            updatingResponseId ===
                                                                                            response.id
                                                                                        }
                                                                                    >
                                                                                        Reject
                                                                                    </button>
                                                                                </div>
                                                                            </div>
                                                                        )}

                                                                    </div>
                                                                ))}

                                                            </div>
                                                        )}

                                                    </div>
                                                )}

                                                {/* Find Nearby Donors */}
                                                {request.status?.toLowerCase() ===
                                                    'open' && (
                                                    <button
                                                        className="btn btn-outline-danger w-100 mt-3"
                                                        onClick={() =>
                                                            loadNearbyDonors(
                                                                request.id
                                                            )
                                                        }
                                                        disabled={
                                                            loadingDonors ===
                                                            request.id
                                                        }
                                                    >
                                                        {loadingDonors ===
                                                        request.id
                                                            ? 'Finding Donors...'
                                                            : 'Find Nearby Donors'}
                                                    </button>
                                                )}

                                                {/* Nearby Donor Errors */}
                                                {donorErrors[request.id] && (
                                                    <div className="alert alert-danger mt-3">
                                                        {
                                                            donorErrors[
                                                                request.id
                                                            ]
                                                        }
                                                    </div>
                                                )}

                                                {/* Nearby Donors Results */}
                                                {nearbyDonors[request.id] && (
                                                    <div className="nearby-donors-section mt-4">

                                                        <h5 className="fw-bold mb-3">
                                                            Nearby Donors
                                                        </h5>

                                                        {/* Total Donors */}
                                                        <p className="text-muted small">
                                                            {
                                                                nearbyDonors[
                                                                    request.id
                                                                ].totalCount
                                                            } donor(s) found
                                                        </p>

                                                        {/* No Donors */}
                                                        {nearbyDonors[
                                                            request.id
                                                        ].items.length ===
                                                            0 && (
                                                            <div className="alert alert-warning mb-0">
                                                                No eligible donors found within the selected radius.
                                                            </div>
                                                        )}

                                                        {/* Donor List */}
                                                        {nearbyDonors[
                                                            request.id
                                                        ].items.length > 0 && (
                                                            <div className="d-flex flex-column gap-2">

                                                                {nearbyDonors[
                                                                    request.id
                                                                ].items.map(donor => (
                                                                    <div
                                                                        className="nearby-donor-card"
                                                                        key={donor.donorId}
                                                                    >
                                                                        <div className="d-flex justify-content-between align-items-center">

                                                                            <div>
                                                                                <strong>
                                                                                    Donor #{donor.donorId}
                                                                                </strong>

                                                                                <div className="text-muted small">
                                                                                    Blood Group: {donor.bloodGroup}
                                                                                </div>
                                                                            </div>

                                                                            <span className="nearby-donor-distance">
                                                                                {donor.distanceKm} km
                                                                            </span>

                                                                        </div>
                                                                    </div>
                                                                ))}

                                                            </div>
                                                        )}
                                                    </div>
                                                )}

                                            </div>
                                        </div>
                                    </div>
                                )
                            })}
                        </div>
                    )}
                </>
            )}
        </main>
    )
}

export default RequesterDashboard