function FeatureCard({ icon, title, description }) {
  return (
    <div className="col-md-4">
      <div className="card shadow-sm h-100 border-0">
        <div className="card-body text-center p-4">
          <div className="display-5">{icon}</div>

          <h4 className="mt-3">{title}</h4>

          <p className="text-secondary">{description}</p>
        </div>
      </div>
    </div>
  )
}

export default FeatureCard