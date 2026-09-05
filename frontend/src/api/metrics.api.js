import axiosInstance from './axiosInstance';

export const metricsApi = {
  get: () => axiosInstance.get('/metrics'),
};
