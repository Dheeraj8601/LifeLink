function HeroSection() {
  return (
    <section className="text-center py-5">
      <h1 className="display-4 fw-bold">
        Every Drop Can <span className="text-danger">Save a Life</span>
      </h1>

      <p className="lead text-secondary mt-3">
        Connect blood donors with people in need. Together, we can make
        emergency blood donation faster and easier.
      </p>

      <div className="mt-4">
        <button className="btn btn-danger btn-lg me-3">
          Find Blood Donors
        </button>

        <button className="btn btn-outline-danger btn-lg">
          Become a Donor
        </button>
      </div>
    </section>
  )
}

export default HeroSection