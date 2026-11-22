import { useEffect, useState } from 'react'
import donorService from '../services/donorService'

const responseStatusMap = {
  1: { label: 'Pending', className: 'bg-warning text-dark' },
  2: { label: 'Accepted', className: 'bg-success' },
  3: { label: 'Rejected', className: 'bg-danger' },
  4: { label: 'Withdrawn', className: 'bg-secondary' },
}

const requestStatusMap = {
  1: 'Open',
  2: 'Partially Fulfilled',
  3: 'Fulfilled',
  4: 'Cancelled',
}

const urgencyMap = {
  1: 'Normal',
  2: 'Urgent',
  3: 'Critical',
}

function MyResponses() {
  const [responses, setResponses] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    loadResponses()
  }, [])

  const loadResponses = async () => {
    try {
      setLoading(true)
      setError('')

      const data = await donorService.getMyResponses()
      setResponses(data)
    } catch (err) {
      setError(
        err.response?.data?.message ||
        'Failed to load your responses.'
      )
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="text-center py-4">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
        <p className="mt-2">Loading your responses...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="alert alert-danger">
        {error}
        <button
          className="btn btn-sm btn-outline-danger ms-3"
          onClick={loadResponses}
        >
          Retry
        </button>
      </div>
    )
  }

  if (responses.length === 0) {
    return (
      <div className="card shadow-sm">
        <div className="card-body text-center py-5">
          <h5>No responses yet</h5>
          <p className="text-muted mb-0">
            When you respond to blood requests, they will appear here.
          </p>
        </div>
      </div>
    )
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h4>My Responses</h4>
        <span className="badge bg-primary">
          {responses.length} response{responses.length !== 1 ? 's' : ''}
        </span>
      </div>

      <div className="row g-3">
        {responses.map((response) => {
          const responseStatus =
            responseStatusMap[response.responseStatus] || {
              label: 'Unknown',
              className: 'bg-secondary',
            }

          const requestStatus =
            requestStatusMap[response.requestStatus] || 'Unknown'

          const urgency =
            urgencyMap[response.urgency] || 'Unknown'

          return (
            <div
              className="col-12"
              key={response.responseId}
            >
              <div className="card shadow-sm">
                <div className="card-body">
                  <div className="d-flex justify-content-between align-items-start flex-wrap gap-2">
                    <div>
                      <h5 className="mb-1">
                        {response.hospitalName}
                      </h5>

                      <p className="text-muted mb-2">
                        {response.hospitalAddress}
                      </p>
                    </div>

                    <span
                      className={`badge ${responseStatus.className}`}
                    >
                      {responseStatus.label}
                    </span>
                  </div>

                  <hr />

                  <div className="row g-3">
                    <div className="col-6 col-md-3">
                      <small className="text-muted">Blood Group</small>
                      <div className="fw-bold text-danger">
                        {response.bloodGroup}
                      </div>
                    </div>

                    <div className="col-6 col-md-3">
                      <small className="text-muted">Units Required</small>
                      <div className="fw-bold">
                        {response.unitsRequired}
                      </div>
                    </div>

                    <div className="col-6 col-md-3">
                      <small className="text-muted">Urgency</small>
                      <div className="fw-bold">
                        {urgency}
                      </div>
                    </div>

                    <div className="col-6 col-md-3">
                      <small className="text-muted">Request Status</small>
                      <div className="fw-bold">
                        {requestStatus}
                      </div>
                    </div>
                  </div>

                  <div className="mt-3 pt-2 border-top">
                    <small className="text-muted">
                      Responded on:{' '}
                      {new Date(response.respondedAt).toLocaleString()}
                    </small>
                  </div>
                </div>
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}

export default MyResponses