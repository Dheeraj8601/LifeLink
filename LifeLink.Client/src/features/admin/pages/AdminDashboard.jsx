
import Navbar from "../../../components/layout/Navbar";
import Footer from "../../../components/layout/Footer";
import PendingDonors from "../components/PendingDonors";

function AdminDashboard() {
    return (
        <>
            <Navbar />

            <main className="container py-5">
                <div className="mb-4">
                    <h1>Admin Dashboard</h1>
                    <p className="text-muted">
                        Manage donor registrations and verification.
                    </p>
                </div>

                <PendingDonors />
            </main>

            <Footer />
        </>
    );
}

export default AdminDashboard;
