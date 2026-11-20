import Navbar from '../../../components/layout/Navbar'
import Footer from '../../../components/layout/Footer'
import RegisterForm from '../components/RegisterForm'

function RegisterPage() {
  return (
    <>
      <Navbar />

      <main>
        <RegisterForm />
      </main>

      <Footer />
    </>
  )
}

export default RegisterPage