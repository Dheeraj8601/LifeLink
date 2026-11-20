import Navbar from '../../../components/layout/Navbar'
import Footer from '../../../components/layout/Footer'
import HeroSection from '../components/HeroSection'
import FeaturesSection from '../components/FeaturesSection'

function HomePage() {
  return (
    <div className="lifelink-app">
      <Navbar />

      <main className="container py-5">
        <HeroSection />
        <FeaturesSection />
      </main>

      <Footer />
    </div>
  )
}

export default HomePage