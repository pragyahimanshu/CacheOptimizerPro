import { Component } from 'react';

export default class ErrorBoundary extends Component {
  state = { hasError: false };
  static getDerivedStateFromError() { return { hasError: true }; }
  render() { return this.state.hasError ? <div className="fatal-error"><strong>Something went wrong.</strong><button onClick={() => window.location.reload()}>Reload application</button></div> : this.props.children; }
}
