import { useEffect, useState } from 'react'
import donorService from '../services/donorService'

function DonorOpenRequests() {
    const [requests, setRequests] = useState([])
    const [isLoading, setIsLoading] = useState(true)
    const [error, setError] = useState(null)

    const [filter, setFilter] = useState('all')

    // Pagination state
    const [pageNumber, setPageNumber] = useState(1)
    const [totalPages, setTotalPages] = useState(1)

    const pageSize = 10

    const [submittingRequestId, setSubmittingRequestId] = useState(null)
    const [submittedRequests, setSubmittedRequests] = useState([])
    const [responseMessages, setResponseMessages] = useState({})

    // Fetch requests whenever the page changes
    useEffect(() => {
        const fetchOpenRequests = async () => {
            setIsLoading(true)
            setError(null)

            try {
                const result = await donorService.getOpenBloodRequests(
                    pageNumber,
                    pageSize
                )

                setRequests(result.items || [])
                setTotalPages(result.totalPages || 1)
            } catch (error) {
                setError(
                    error.response?.data?.message ||
                    'Failed to load open blood requests.'
                )
            } finally {
                setIsLoading(false)
            }
        }

        fetchOpenRequests()
    }, [pageNumber])

    // Submit donor response
    const handleRespond = async (requestId) => {
        setSubmittingRequestId(requestId)

        setResponseMessages(previous => ({
            ...previous,
            [requestId]: null,
        }))

        try {
            const result = await donorService.submitResponse(requestId)

            setSubmittedRequests(previous => [
                ...previous,
                requestId,
            ])

            setResponseMessages(previous => ({
                ...previous,
                [requestId]: {
                    type: 'success',
                    message:
                        result.message ||
                        'Your response has been submitted successfully.',
                },
            }))
        } catch (error) {
            setResponseMessages(previous => ({
                ...previous,
                [requestId]: {
                    type: 'error',
                    message:
                        error.response?.data?.message ||
                        'Failed to submit your response.',
                },
            }))
        } finally {
            setSubmittingRequestId(null)
        }
    }

    // Filter requests by urgency
    const filteredRequests = requests.filter(request => {
        if (filter === 'all') {
            return true
        }

        return request.urgency?.toLowerCase() === filter
    })

    const handlePreviousPage = () => {
        if (pageNumber > 1) {
            setPageNumber(previous => previous - 1)
        }
    }

    const handleNextPage = () => {
        if (pageNumber < totalPages) {
            setPageNumber(previous => previous + 1)
        }
    }

    return (
        <section className="donor-open-requests mt-5">

            {/* Header */}
            <div className="d-flex flex-wrap justify-content-between align-items-center mb-4 gap-3">
                <h2 className="mb-0">Open Blood Requests</h2>

                {!isLoading && !error && requests.length > 0 && (
                    <div className="d-flex align-items-center gap-2">
                        <label
                            htmlFor="urgencyFilter"
                            className="fw-semibold"
                        >
                            Filter:
                        </label>

                        <select
                            id="urgencyFilter"
                            className="form-select"
                            style={{ width: '180px' }}
                            value={filter}
                            onChange={(event) => setFilter(event.target.value)}
                        >
                            <option value="all">All Requests</option>
                            <option value="critical">Critical</option>
                            <option value="urgent">Urgent</option>
                            <option value="normal">Normal</option>
                        </select>
                    </div>
                )}
            </div>

            {/* Loading */}
            {isLoading && (
                <div className="alert alert-info">
                    Loading blood requests...
                </div>
            )}

            {/* Error */}
            {error && (
                <div className="alert alert-danger">
                    {error}
                </div>
            )}

            {/* Empty State */}
            {!isLoading && !error && requests.length === 0 && (
                <div className="alert alert-info">
                    No open blood requests available currently.
                </div>
            )}

            {/* No Filter Results */}
            {!isLoading &&
                !error &&
                requests.length > 0 &&
                filteredRequests.length === 0 && (
                    <div className="alert alert-warning">
                        No requests found for this urgency level.
                    </div>
                )}

            {/* Request Cards */}
            {!isLoading && !error && filteredRequests.length > 0 && (
                <div className="row g-4">
                    {filteredRequests.map(request => {
                        const hasSubmittedResponse =
                            submittedRequests.includes(request.id)

                        const isSubmitting =
                            submittingRequestId === request.id

                        const responseMessage =
                            responseMessages[request.id]

                        return (
                            <div
                                className="col-12 col-md-6 col-lg-4"
                                key={request.id}
                            >
                                <div className="card shadow-sm h-100 border-0">
                                    <div className="card-body">

                                        {/* Blood Group & Status */}
                                        <div className="d-flex justify-content-between align-items-start mb-3">
                                            <h3 className="fw-bold text-danger mb-0">
                                                {request.bloodGroup}
                                            </h3>

                                            <span className="badge bg-success">
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
                                                className={`badge ${
                                                    request.urgency?.toLowerCase() === 'critical'
                                                        ? 'bg-danger'
                                                        : request.urgency?.toLowerCase() === 'urgent'
                                                            ? 'bg-warning text-dark'
                                                            : 'bg-secondary'
                                                }`}
                                            >
                                                {request.urgency}
                                            </span>
                                        </p>

                                        {/* Address */}
                                        {request.hospitalAddress && (
                                            <p className="mb-2">
                                                <strong>Address:</strong>{' '}
                                                {request.hospitalAddress}
                                            </p>
                                        )}

                                        {/* Description */}
                                        {request.description && (
                                            <p className="mb-2">
                                                <strong>Details:</strong>{' '}
                                                {request.description}
                                            </p>
                                        )}

                                        {/* Created Date */}
                                        <p className="text-muted small mt-3 mb-0">
                                            Created:{' '}
                                            {new Date(
                                                request.createdAt
                                            ).toLocaleDateString()}
                                        </p>

                                        {/* Response Message */}
                                        {responseMessage && (
                                            <div
                                                className={`alert ${
                                                    responseMessage.type === 'success'
                                                        ? 'alert-success'
                                                        : 'alert-danger'
                                                } mt-3 mb-0`}
                                                role="alert"
                                            >
                                                {responseMessage.message}
                                            </div>
                                        )}

                                        {/* Respond Button */}
                                        <button
                                            type="button"
                                            className={`btn w-100 mt-3 ${
                                                hasSubmittedResponse
                                                    ? 'btn-success'
                                                    : 'btn-danger'
                                            }`}
                                            onClick={() =>
                                                handleRespond(request.id)
                                            }
                                            disabled={
                                                hasSubmittedResponse ||
                                                isSubmitting
                                            }
                                        >
                                            {isSubmitting
                                                ? 'Submitting Response...'
                                                : hasSubmittedResponse
                                                    ? 'Response Submitted'
                                                    : 'Respond to Request'}
                                        </button>

                                    </div>
                                </div>
                            </div>
                        )
                    })}
                </div>
            )}

            {/* Pagination */}
            {!isLoading && !error && totalPages > 1 && (
                <div className="d-flex justify-content-center align-items-center gap-3 mt-5">

                    <button
                        className="btn btn-outline-danger"
                        onClick={handlePreviousPage}
                        disabled={pageNumber === 1}
                    >
                        ← Previous
                    </button>

                    <span className="fw-semibold">
                        Page {pageNumber} of {totalPages}
                    </span>

                    <button
                        className="btn btn-outline-danger"
                        onClick={handleNextPage}
                        disabled={pageNumber === totalPages}
                    >
                        Next →
                    </button>

                </div>
            )}

        </section>
    )
}

export default DonorOpenRequests