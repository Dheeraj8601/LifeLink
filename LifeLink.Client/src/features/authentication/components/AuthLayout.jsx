function AuthLayout({ title, subtitle, children }) {
  return (
    <div className="container py-5">
      <div className="row justify-content-center">
        <div className="col-12 col-md-8 col-lg-5">
          <div className="card shadow border-0">
            <div className="card-body p-4 p-md-5">
              <div className="text-center mb-4">
                <h2 className="fw-bold text-danger">{title}</h2>

                <p className="text-secondary">{subtitle}</p>
              </div>

              {children}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default AuthLayout