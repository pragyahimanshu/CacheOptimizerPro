import PropTypes from 'prop-types';

export default function Modal({ title, children, onClose }) {
  return <div className="modal-backdrop" role="presentation" onClick={onClose}>
    <div className="modal" role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
      <div className="modal-heading"><h2>{title}</h2><button className="icon-button" onClick={onClose} aria-label="Close">x</button></div>
      {children}
    </div>
  </div>;
}

Modal.propTypes = { title: PropTypes.string.isRequired, children: PropTypes.node.isRequired, onClose: PropTypes.func.isRequired };
