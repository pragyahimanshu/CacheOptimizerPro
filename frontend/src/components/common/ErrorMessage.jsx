import PropTypes from 'prop-types';

export default function ErrorMessage({ message = 'Something went wrong.' }) {
  return <div className="error-message" role="alert">{message}</div>;
}

ErrorMessage.propTypes = { message: PropTypes.string };
