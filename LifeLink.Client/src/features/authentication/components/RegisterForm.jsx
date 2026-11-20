import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'

import FormInput from '../../../components/common/FormInput'
import AuthLayout from './AuthLayout'
import cityService from '../../../core/services/cityService'
import authService from '../services/authService'

function RegisterForm() {
    const navigate = useNavigate()

    // Common fields
    const [role, setRole] = useState('Donor')
    const [fullName, setFullName] = useState('')
    const [email, setEmail] = useState('')
    const [phoneNumber, setPhoneNumber] = useState('')
    const [password, setPassword] = useState('')
    const [confirmPassword, setConfirmPassword] = useState('')
    const [cityId, setCityId] = useState('')

    // Donor-specific fields
    const [bloodGroup, setBloodGroup] = useState('')
    const [latitude, setLatitude] = useState('')
    const [longitude, setLongitude] = useState('')

    // Location state
    const [locationStatus, setLocationStatus] = useState('')
    const [isLocating, setIsLocating] = useState(false)

    // City state
    const [cities, setCities] = useState([])
    const [isLoadingCities, setIsLoadingCities] = useState(true)
    const [cityError, setCityError] = useState('')

    // Form state
    const [isRegistering, setIsRegistering] = useState(false)
    const [error, setError] = useState('')

    // Load cities
    useEffect(() => {
        const loadCities = async () => {
            try {
                const cityData = await cityService.getAllCities()
                setCities(cityData)
            } catch (error) {
                setCityError(error.message || 'Failed to load cities.')
            } finally {
                setIsLoadingCities(false)
            }
        }

        loadCities()
    }, [])

    // Reset donor-specific fields when switching roles
    function handleRoleChange(event) {
        const selectedRole = event.target.value

        setRole(selectedRole)
        setBloodGroup('')
        setLatitude('')
        setLongitude('')
        setLocationStatus('')
        setError('')
    }

    // Get current location - Donor only
    function getCurrentLocation() {
        if (!navigator.geolocation) {
            setLocationStatus(
                'Geolocation is not supported by your browser.'
            )
            return
        }

        setIsLocating(true)
        setLocationStatus('Detecting your location...')

        navigator.geolocation.getCurrentPosition(
            (position) => {
                const { latitude, longitude } = position.coords

                setLatitude(latitude.toString())
                setLongitude(longitude.toString())

                setLocationStatus('Location detected successfully!')
                setIsLocating(false)
            },
            (error) => {
                setIsLocating(false)

                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        setLocationStatus(
                            'Location permission denied. Please allow location access.'
                        )
                        break

                    case error.POSITION_UNAVAILABLE:
                        setLocationStatus(
                            'Your location is currently unavailable.'
                        )
                        break

                    case error.TIMEOUT:
                        setLocationStatus(
                            'Location detection timed out. Please try again.'
                        )
                        break

                    default:
                        setLocationStatus(
                            'Unable to detect your location.'
                        )
                }
            },
            {
                enableHighAccuracy: true,
                timeout: 15000,
                maximumAge: 0,
            }
        )
    }

    // Submit registration
    const handleSubmit = async (e) => {
        e.preventDefault()

        setError('')

        // Client-side validation
        if (password !== confirmPassword) {
            setError('Passwords do not match.')
            return
        }

        if (!cityId) {
            setError('Please select your city.')
            return
        }

        setIsRegistering(true)

        try {
            // Requester registration
            if (role === 'Requester') {
                const requesterData = {
                    fullName,
                    email,
                    password,
                    phoneNumber,
                    cityId: Number(cityId),
                }

                await authService.registerRequester(requesterData)
            }

            // Donor registration
            else {
                const donorData = {
                    fullName,
                    email,
                    password,
                    phoneNumber,
                    cityId: Number(cityId),
                    bloodGroup,
                    latitude:
                        latitude === '' ? null : Number(latitude),
                    longitude:
                        longitude === '' ? null : Number(longitude),
                }

                await authService.register(donorData)
            }

            // Registration successful
            navigate('/login', {
                state: {
                    message:
                        `${role} registration successful! Please login with your credentials.`,
                },
            })
        } catch (error) {
            setError(error.message || 'Registration failed.')
        } finally {
            setIsRegistering(false)
        }
    }

    return (
        <AuthLayout
            title="Create Account"
            subtitle="Join LifeLink and help save lives"
        >
            <form onSubmit={handleSubmit}>
                {/* Error Message */}
                {error && (
                    <div className="alert alert-danger" role="alert">
                        {error}
                    </div>
                )}

                {/* Role Selection */}
                <div className="mb-3">
                    <label htmlFor="role" className="form-label">
                        Register As
                    </label>

                    <select
                        id="role"
                        name="role"
                        className="form-select"
                        value={role}
                        onChange={handleRoleChange}
                        required
                    >
                        <option value="Donor">Blood Donor</option>
                        <option value="Requester">Blood Requester</option>
                    </select>

                    <small className="text-muted">
                        {role === 'Donor'
                            ? 'Register to help patients by donating blood.'
                            : 'Register to request blood for patients in need.'}
                    </small>
                </div>

                {/* Full Name */}
                <FormInput
                    label="Full Name"
                    name="fullName"
                    value={fullName}
                    onChange={(event) => setFullName(event.target.value)}
                    placeholder="Enter your full name"
                    required
                />

                {/* Email */}
                <FormInput
                    label="Email Address"
                    type="email"
                    name="email"
                    value={email}
                    onChange={(event) => setEmail(event.target.value)}
                    placeholder="Enter your email"
                    required
                />

                {/* Phone Number */}
                <FormInput
                    label="Phone Number"
                    type="tel"
                    name="phoneNumber"
                    value={phoneNumber}
                    onChange={(event) => setPhoneNumber(event.target.value)}
                    placeholder="Enter your phone number"
                    required
                />

                {/* Donor-specific fields */}
                {role === 'Donor' && (
                    <>
                        {/* Blood Group */}
                        <div className="mb-3">
                            <label
                                htmlFor="bloodGroup"
                                className="form-label"
                            >
                                Blood Group
                            </label>

                            <select
                                id="bloodGroup"
                                name="bloodGroup"
                                className="form-select"
                                value={bloodGroup}
                                onChange={(event) =>
                                    setBloodGroup(event.target.value)
                                }
                                required
                            >
                                <option value="">
                                    Select Blood Group
                                </option>
                                <option value="A+">A+</option>
                                <option value="A-">A-</option>
                                <option value="B+">B+</option>
                                <option value="B-">B-</option>
                                <option value="AB+">AB+</option>
                                <option value="AB-">AB-</option>
                                <option value="O+">O+</option>
                                <option value="O-">O-</option>
                            </select>
                        </div>
                    </>
                )}

                {/* City - Common field */}
                <div className="mb-3">
                    <label htmlFor="cityId" className="form-label">
                        City <span className="text-danger">*</span>
                    </label>

                    <select
                        id="cityId"
                        name="cityId"
                        className="form-select"
                        value={cityId}
                        onChange={(event) => setCityId(event.target.value)}
                        required
                        disabled={isLoadingCities || cities.length === 0}
                    >
                        <option value="">
                            {isLoadingCities
                                ? 'Loading cities...'
                                : 'Select your city'}
                        </option>

                        {cities.map((city) => (
                            <option key={city.id} value={city.id}>
                                {city.name}, {city.state}
                            </option>
                        ))}
                    </select>

                    {cityError && (
                        <div className="text-danger small mt-1">
                            {cityError}
                        </div>
                    )}
                </div>

                {/* Donor-specific location */}
                {role === 'Donor' && (
                    <div className="mb-3">
                        <label className="form-label">
                            Your Location
                        </label>

                        <button
                            type="button"
                            className="btn btn-outline-danger w-100"
                            onClick={getCurrentLocation}
                            disabled={isLocating}
                        >
                            {isLocating ? (
                                <>
                                    <span
                                        className="spinner-border spinner-border-sm me-2"
                                        role="status"
                                    ></span>
                                    Detecting Location...
                                </>
                            ) : (
                                <>
                                    <i className="bi bi-geo-alt me-2"></i>
                                    Use My Current Location
                                </>
                            )}
                        </button>

                        {locationStatus && (
                            <div className="mt-2">
                                <small
                                    className={
                                        latitude !== '' &&
                                        longitude !== ''
                                            ? 'text-success'
                                            : 'text-muted'
                                    }
                                >
                                    {locationStatus}
                                    </small>
                            </div>
                        )}

                        {latitude !== '' && longitude !== '' && (
                            <div className="alert alert-light border mt-2 mb-0">
                                <small>
                                    <strong>Latitude:</strong> {latitude}
                                    <br />
                                    <strong>Longitude:</strong> {longitude}
                                </small>
                            </div>
                        )}
                    </div>
                )}

                {/* Password */}
                <FormInput
                    label="Password"
                    type="password"
                    name="password"
                    value={password}
                    onChange={(event) => setPassword(event.target.value)}
                    placeholder="Create a password"
                    required
                />

                {/* Confirm Password */}
                <FormInput
                    label="Confirm Password"
                    type="password"
                    name="confirmPassword"
                    value={confirmPassword}
                    onChange={(event) =>
                        setConfirmPassword(event.target.value)
                    }
                    placeholder="Confirm your password"
                    required
                />

                {/* Submit */}
                <button
                    type="submit"
                    className="btn btn-danger w-100 mt-3"
                    disabled={isRegistering}
                >
                    {isRegistering ? (
                        <>
                            <span
                                className="spinner-border spinner-border-sm me-2"
                                role="status"
                            ></span>
                            Creating Account...
                        </>
                    ) : (
                        'Create Account'
                    )}
                </button>
            </form>

            {/* Login Link */}
            <div className="text-center mt-4">
                <p className="mb-0">
                    Already have an account?{' '}
                    <Link
                        to="/login"
                        className="text-danger fw-semibold text-decoration-none"
                    >
                        Login
                    </Link>
                </p>
            </div>
        </AuthLayout>
    )
}

export default RegisterForm