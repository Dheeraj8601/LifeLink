import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import MainLayout from '../../../components/layout/MainLayout'
import cityService from '../../../core/services/cityService'
import bloodRequestService from '../services/bloodRequestService'
import './BloodRequestPage.css'

const CreateBloodRequestPage = () => {
    const navigate = useNavigate()

    const [cities, setCities] = useState([])
    const [loadingCities, setLoadingCities] = useState(true)
    const [cityError, setCityError] = useState('')

    const [isGettingLocation, setIsGettingLocation] = useState(false)
    const [locationMessage, setLocationMessage] = useState('')

    const [isSubmitting, setIsSubmitting] = useState(false)
    const [submitError, setSubmitError] = useState('')
    const [successMessage, setSuccessMessage] = useState('')

    const [formData, setFormData] = useState({
        bloodGroup: '',
        unitsRequired: 1,
        urgency: 'Normal',
        hospitalName: '',
        hospitalAddress: '',
        cityId: '',
        latitude: '',
        longitude: '',
        description: '',
    })

    const handleChange = (e) => {
        const { name, value } = e.target

        setFormData((previousData) => ({
            ...previousData,
            [name]: value,
        }))
    }

    const handleGetLocation = () => {
        if (!navigator.geolocation) {
            setLocationMessage(
                'Geolocation is not supported by your browser.'
            )
            return
        }

        setIsGettingLocation(true)
        setLocationMessage('Getting your location...')

        navigator.geolocation.getCurrentPosition(
            (position) => {
                const { latitude, longitude } = position.coords

                setFormData((previousData) => ({
                    ...previousData,
                    latitude,
                    longitude,
                }))

                setLocationMessage(
                    'Location captured successfully.'
                )

                setIsGettingLocation(false)
            },
            (error) => {
                console.error('Geolocation error:', error)

                setLocationMessage(
                    'Unable to get location. Please allow location access and try again.'
                )

                setIsGettingLocation(false)
            },
            {
                enableHighAccuracy: true,
                timeout: 10000,
                maximumAge: 0,
            }
        )
    }

    // Submit blood request
    const handleSubmit = async (e) => {
        e.preventDefault()

        setSubmitError('')
        setSuccessMessage('')

        // Validate location coordinates
        if (
            (formData.latitude === '') !==
            (formData.longitude === '')
        ) {
            setSubmitError(
                'Please provide both latitude and longitude.'
            )
            return
        }

        const requestData = {
            bloodGroup: formData.bloodGroup,
            unitsRequired: Number(formData.unitsRequired),
            urgency: formData.urgency,
            hospitalName: formData.hospitalName.trim(),
            hospitalAddress: formData.hospitalAddress.trim(),
            cityId: Number(formData.cityId),
            latitude:
                formData.latitude === ''
                    ? null
                    : Number(formData.latitude),
            longitude:
                formData.longitude === ''
                    ? null
                    : Number(formData.longitude),
            description: formData.description.trim() || null,
        }

        setIsSubmitting(true)

        try {
            const createdRequest =
                await bloodRequestService.createBloodRequest(
                    requestData
                )

            console.log('Blood request created:', createdRequest)

            setSuccessMessage(
                'Blood request created successfully!'
            )

            // Reset form after successful submission
            setFormData({
                bloodGroup: '',
                unitsRequired: 1,
                urgency: 'Normal',
                hospitalName: '',
                hospitalAddress: '',
                cityId: '',
                latitude: '',
                longitude: '',
                description: '',
            })

            setLocationMessage('')
        } catch (error) {
            console.error('Blood request creation failed:', error)

            setSubmitError(
                error.message ||
                'Failed to create blood request. Please try again.'
            )
        } finally {
            setIsSubmitting(false)
        }
    }

    useEffect(() => {
        const loadCities = async () => {
            try {
                const data = await cityService.getAllCities()
                setCities(data)
            } catch (error) {
                setCityError('Failed to load cities.')
                console.error('Error loading cities:', error)
            } finally {
                setLoadingCities(false)
            }
        }

        loadCities()
    }, [])

    return (
        <MainLayout>
            <div className="blood-request-page">
                <div className="container">
                    <div className="blood-request-container">

                        <div className="blood-request-card">

                            {/* Page Header */}
                            <div className="blood-request-header">
                                <h2 className="blood-request-title">
                                    Request Blood
                                </h2>

                                <p className="blood-request-subtitle">
                                    Provide the details of your blood requirement.
                                    We'll help you connect with nearby donors.
                                </p>
                            </div>

                            <div className="blood-request-body">

                                {/* Success Message */}
                                {successMessage && (
                                    <div
                                        className="alert alert-success blood-request-alert"
                                        role="alert"
                                    >
                                        {successMessage}
                                    </div>
                                )}

                                {/* Blood Details */}
                                <section className="blood-request-section">
                                    <h5 className="blood-request-section-title">
                                        Blood Details
                                    </h5>

                                    <div className="row">

                                        {/* Blood Group */}
                                        <div className="col-md-6 mb-3">
                                            <label className="form-label">
                                                Blood Group
                                            </label>

                                            <select
                                                name="bloodGroup"
                                                className="form-select"
                                                value={formData.bloodGroup}
                                                onChange={handleChange}
                                                required
                                            >
                                                <option value="">
                                                    Select blood group
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

                                        {/* Units Required */}
                                        <div className="col-md-6 mb-3">
                                            <label className="form-label">
                                                Units Required
                                            </label>

                                            <input
                                                name="unitsRequired"
                                                type="number"
                                                className="form-control"
                                                min="1"
                                                max="100"
                                                value={formData.unitsRequired}
                                                onChange={handleChange}
                                                required
                                            />
                                        </div>

                                        {/* Urgency */}
                                        <div className="col-md-6 mb-3">
                                            <label className="form-label">
                                                Urgency
                                            </label>

                                            <select
                                                name="urgency"
                                                className="form-select"
                                                value={formData.urgency}
                                                onChange={handleChange}
                                                required
                                            >
                                                <option value="Normal">
                                                    Normal
                                                </option>
                                                <option value="Urgent">
                                                    Urgent
                                                </option>
                                                <option value="Critical">
                                                    Critical
                                                </option>
                                            </select>
                                        </div>

                                    </div>
                                </section>

                                {/* Hospital Details */}
                                <section className="blood-request-section">
                                    <h5 className="blood-request-section-title">
                                        Hospital Details
                                    </h5>

                                    {/* Hospital Name */}
                                    <div className="mb-3">
                                        <label className="form-label">
                                            Hospital Name
                                        </label>

                                        <input
                                            name="hospitalName"
                                            type="text"
                                            className="form-control"
                                            placeholder="Enter hospital name"
                                            value={formData.hospitalName}
                                            onChange={handleChange}
                                            minLength="2"
                                            maxLength="200"
                                            required
                                        />
                                    </div>

                                    {/* Hospital Address */}
                                    <div className="mb-3">
                                        <label className="form-label">
                                            Hospital Address
                                        </label>

                                        <textarea
                                            name="hospitalAddress"
                                            className="form-control"
                                            rows="3"
                                            placeholder="Enter hospital address"
                                            value={formData.hospitalAddress}
                                            onChange={handleChange}
                                            minLength="5"
                                            maxLength="500"
                                            required
                                        />
                                    </div>

                                    {/* City */}
                                    <div className="mb-3">
                                        <label className="form-label">
                                            City
                                        </label>

                                        <select
                                            name="cityId"
                                            className="form-select"
                                            value={formData.cityId}
                                            onChange={handleChange}
                                            disabled={loadingCities}
                                            required
                                        >
                                            <option value="">
                                                {loadingCities
                                                    ? 'Loading cities...'
                                                    : 'Select city'}
                                            </option>

                                            {cities.map((city) => (
                                                <option
                                                    key={city.id}
                                                    value={city.id}
                                                >
                                                    {city.name}
                                                </option>
                                            ))}
                                        </select>

                                        {cityError && (
                                            <div className="text-danger mt-1">
                                                {cityError}
                                            </div>
                                        )}
                                    </div>
                                </section>

                                {/* Location */}
                                <section className="blood-request-section">
                                    <h5 className="blood-request-section-title">
                                        Location
                                    </h5>

                                    <p className="text-muted small mb-3">
                                        Your location helps us find nearby blood donors.
                                    </p>

                                    <button
                                        type="button"
                                        className="btn blood-request-location-button"
                                        onClick={handleGetLocation}
                                        disabled={isGettingLocation}
                                    >
                                        {isGettingLocation
                                            ? 'Getting Location...'
                                            : '📍 Use My Current Location'}
                                    </button>

                                    {locationMessage && (
                                        <div className="mt-2">
                                            <small className="text-muted">
                                                {locationMessage}
                                            </small>
                                        </div>
                                    )}

                                    {formData.latitude !== '' &&
                                        formData.longitude !== '' && (
                                            <div className="alert alert-success mt-3 blood-request-alert">
                                                <strong>
                                                    Location captured
                                                </strong>

                                                <div>
                                                    Latitude: {formData.latitude}
                                                </div>

                                                <div>
                                                    Longitude: {formData.longitude}
                                                </div>
                                            </div>
                                        )}
                                </section>

                                {/* Additional Information */}
                                <section className="blood-request-section">
                                    <h5 className="blood-request-section-title">
                                        Additional Information
                                    </h5>

                                    <div className="mb-3">
                                        <label className="form-label">
                                            Description (Optional)
                                        </label>

                                        <textarea
                                            name="description"
                                            className="form-control"
                                            rows="4"
                                            maxLength="1000"
                                            placeholder="Enter any additional information about the blood requirement..."
                                            value={formData.description}
                                            onChange={handleChange}
                                        />

                                        <small className="text-muted">
                                            {formData.description.length}/1000 characters
                                        </small>
                                    </div>
                                </section>

                                {/* Submission */}
                                <section className="mt-4">

                                    {submitError && (
                                        <div
                                            className="alert alert-danger blood-request-alert"
                                            role="alert"
                                        >
                                            {submitError}
                                        </div>
                                    )}

                                    <button
                                        type="button"
                                        className="btn blood-request-submit-button w-100"
                                        onClick={handleSubmit}
                                        disabled={isSubmitting}
                                    >
                                        {isSubmitting
                                            ? 'Submitting Request...'
                                            : 'Submit Blood Request'}
                                    </button>

                                </section>

                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </MainLayout>
    )
}

export default CreateBloodRequestPage