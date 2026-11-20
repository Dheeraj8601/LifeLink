import Navbar from './Navbar'
import Footer from './Footer'

function MainLayout({ children }) {
    return (
        <div className="lifelink-app">
            <Navbar />

            <main className="flex-grow-1">
                {children}
            </main>

            <Footer />
        </div>
    )
}

export default MainLayout