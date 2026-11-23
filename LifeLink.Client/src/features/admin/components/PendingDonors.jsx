
import { useEffect, useState } from "react";
import adminService from "../services/adminService";
import "./PendingDonors.css";

const PendingDonors = () => {
    const [donors, setDonors] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [processingDonorId, setProcessingDonorId] = useState(null);

    const [rejectingDonor, setRejectingDonor] = useState(null);
    const [rejectionReason, setRejectionReason] = useState("");

    const fetchPendingDonors = async () => {
        try {
            setLoading(true);
            setError("");

            const data = await adminService.getPendingDonors();
            setDonors(data);
        } catch (err) {
            setError(
                err.response?.data?.message ||
                "Failed to load pending donors."
            );
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchPendingDonors();
    }, []);

    const handleVerify = async (donorId) => {
        const confirmed = window.confirm(
            "Are you sure you want to verify this donor?"
        );

        if (!confirmed) return;

        try {
            setProcessingDonorId(donorId);

            await adminService.verifyDonor(donorId);

            setDonors((currentDonors) =>
                currentDonors.filter(
                    (donor) => donor.donorId !== donorId
                )
            );
        } catch (err) {
            alert(
                err.response?.data?.message ||
                "Failed to verify donor."
            );
        } finally {
            setProcessingDonorId(null);
        }
    };

    const handleReject = async () => {
        if (!rejectionReason.trim()) {
            alert("Please provide a rejection reason.");
            return;
        }

        try {
            setProcessingDonorId(rejectingDonor.donorId);

            await adminService.rejectDonor(
                rejectingDonor.donorId,
                rejectionReason.trim()
            );

            setDonors((currentDonors) =>
                currentDonors.filter(
                    (donor) =>
                        donor.donorId !== rejectingDonor.donorId
                )
            );

            setRejectingDonor(null);
            setRejectionReason("");
        } catch (err) {
            alert(
                err.response?.data?.message ||
                "Failed to reject donor."
            );
        } finally {
            setProcessingDonorId(null);
        }
    };

    if (loading) {
        return (
            <div className="admin-pending-loading">
                Loading pending donors...
            </div>
        );
    }

    return (
        <div className="admin-pending-donors">
            <div className="admin-pending-header">
                <div>
                    <h2>Donor Verification</h2>
                    <p>
                        Review and verify donor registrations.
                    </p>
                </div>

                <button
                    className="admin-refresh-button"
                    onClick={fetchPendingDonors}
                    disabled={loading}
                >
                    Refresh
                </button>
            </div>

            {error && (
                <div className="admin-error-message">
                    {error}
                </div>
            )}

            <div className="admin-pending-count">
                Pending verification: <strong>{donors.length}</strong>
            </div>

            {donors.length === 0 ? (
                <div className="admin-empty-state">
                    <h3>All caught up! 🎉</h3>
                    <p>There are no donors pending verification.</p>
                </div>
            ) : (
                <div className="admin-donors-grid">
                    {donors.map((donor) => (
                        <div
                            className="admin-donor-card"
                            key={donor.donorId}
                        >
                            <div className="admin-donor-card-header">
                                <span className="admin-blood-group">
                                    {donor.bloodGroup}
                                </span>

                                <span className="admin-pending-badge">
                                    Pending
                                </span>
                            </div>

                            <h3>{donor.fullName}</h3>

                            <div className="admin-donor-details">
                                <p>
                                    <strong>Email:</strong> {donor.email}
                                </p>

                                <p>
                                    <strong>Phone:</strong> {donor.phoneNumber}
                                </p>

                                <p>
                                    <strong>Location:</strong>{" "}
                                    {donor.city}, {donor.state}
                                </p>

                                <p>
                                    <strong>Availability:</strong>{" "}
                                    {donor.isAvailable ? "Available" : "Unavailable"}
                                </p>

                                <p>
                                    <strong>Registered:</strong>{" "}
                                    {donor.registeredAt
                                        ? new Date(donor.registeredAt).toLocaleDateString()
                                        : "N/A"}
                                </p>
                            </div>

                            <div className="admin-donor-actions">
                                <button
                                    className="admin-verify-button"
                                    onClick={() => handleVerify(donor.donorId)}
                                    disabled={processingDonorId === donor.donorId}
                                >
                                    {processingDonorId === donor.donorId
                                        ? "Processing..."
                                        : "Verify"}
                                </button>

                                <button
                                    className="admin-reject-button"
                                    onClick={() => {
                                        setRejectingDonor(donor);
                                        setRejectionReason("");
                                    }}
                                    disabled={processingDonorId === donor.donorId}
                                >
                                    Reject
                                </button>
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {rejectingDonor && (
                <div className="admin-modal-overlay">
                    <div className="admin-rejection-modal">
                        <h3>Reject Donor</h3>

                        <p>
                            Reject registration for{" "}
                            <strong>{rejectingDonor.fullName}</strong>?
                        </p>

                        <label htmlFor="rejectionReason">
                            Rejection reason
                        </label>

                        <textarea
                            id="rejectionReason"
                            value={rejectionReason}
                            onChange={(e) =>
                                setRejectionReason(e.target.value)
                            }
                            placeholder="Enter the reason for rejection..."
                            rows={4}
                            maxLength={500}
                        />

                        <div className="admin-modal-actions">
                            <button
                                className="admin-cancel-button"
                                onClick={() => {
                                    setRejectingDonor(null);
                                    setRejectionReason("");
                                }}
                            >
                                Cancel
                            </button>

                            <button
                                className="admin-confirm-reject-button"
                                onClick={handleReject}
                                disabled={
                                    processingDonorId === rejectingDonor.donorId
                                }
                            >
                                {processingDonorId === rejectingDonor.donorId
                                    ? "Processing..."
                                    : "Confirm Rejection"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default PendingDonors;
