import Navbar from '../../../components/layout/Navbar'
import Footer from '../../../components/layout/Footer'
import LoginForm from '../components/LoginForm'

function LoginPage() {
  return (
    <>
      <Navbar />

      <main>
        <LoginForm />
      </main>

      <Footer />
    </>
  )
}

export default LoginPage