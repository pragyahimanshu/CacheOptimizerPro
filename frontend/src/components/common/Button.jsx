import PropTypes from 'prop-types';

export default function Button({ children, variant = 'primary', type = 'button', ...props }) {
  return <button className={`button button-${variant}`} type={type} {...props}>{children}</button>;
}

Button.propTypes = { children: PropTypes.node.isRequired, variant: PropTypes.string, type: PropTypes.string };
