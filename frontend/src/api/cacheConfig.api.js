import axiosInstance from './axiosInstance';

export const cacheConfigApi = {
  get: () => axiosInstance.get('/cache-config'),
  update: (payload) => axiosInstance.post('/cache-config', payload),
};
