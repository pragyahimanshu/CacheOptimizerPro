import PropTypes from 'prop-types';

export default function Input({ label, ...props }) {
  return <label className="field">{label}<input {...props} /></label>;
}

Input.propTypes = { label: PropTypes.string.isRequired };
