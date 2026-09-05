import PropTypes from 'prop-types';

export default function Card({ children, className = '' }) {
  return <section className={`card ${className}`}>{children}</section>;
}

Card.propTypes = { children: PropTypes.node.isRequired, className: PropTypes.string };
