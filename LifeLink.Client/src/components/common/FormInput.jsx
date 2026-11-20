function FormInput({
    label,
    type,
    name,
    value,
    onChange,
    placeholder,
    required,
    disabled = false,
}) {
    return (
        <div className="mb-3">
            <label htmlFor={name} className="form-label">
                {label}
            </label>

            <input
                id={name}
                type={type}
                name={name}
                value={value}
                onChange={onChange}
                placeholder={placeholder}
                required={required}
                disabled={disabled}
                className="form-control"
            />
        </div>
    )
}

export default FormInput