import FeatureCard from './FeatureCard'

function FeaturesSection() {
  return (
    <section className="row g-4 mt-4">
      <FeatureCard
        icon="🩸"
        title="Find Donors"
        description="Find nearby eligible donors based on blood group and location."
      />

      <FeatureCard
        icon="📍"
        title="Nearby Matching"
        description="Discover donors near your hospital using location-based matching."
      />

      <FeatureCard
        icon="🤝"
        title="Save Lives"
        description="Connect requesters and donors to make blood donation easier."
      />
    </section>
  )
}

export default FeaturesSection