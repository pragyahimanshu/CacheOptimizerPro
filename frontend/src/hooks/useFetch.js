import { useEffect, useState } from 'react';

/** Fetches async data and exposes loading/error state. */
export function useFetch(fetcher, dependencies = []) {
  const [state, setState] = useState({ data: null, loading: true, error: null });

  useEffect(() => {
    let active = true;
    setState({ data: null, loading: true, error: null });
    fetcher()
      .then((response) => active && setState({ data: response.data, loading: false, error: null }))
      .catch((error) => active && setState({ data: null, loading: false, error }));
    return () => { active = false; };
  }, dependencies);

  return state;
}
