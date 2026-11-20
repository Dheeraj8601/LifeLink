function DashboardCard({ title, value, icon, description }) {
    return (
        <div className="card h-100 shadow-sm border-0">
            <div className="card-body">
                <div className="d-flex justify-content-between align-items-start">
                    <div>
                        <p className="text-muted mb-2">{title}</p>

                        <h3 className="fw-bold mb-2">
                            {value}
                        </h3>

                        <small className="text-muted">
                            {description}
                        </small>
                    </div>

                    <div className="fs-2">
                        {icon}
                    </div>
                </div>
            </div>
        </div>
    )
}

export default DashboardCard