import axiosInstance from './axiosInstance';

export const productsApi = {
  list: () => axiosInstance.get('/products'),
  get: (id) => axiosInstance.get(`/products/${id}`),
  create: (payload) => axiosInstance.post('/products', payload),
  update: (id, payload) => axiosInstance.put(`/products/${id}`, payload),
  remove: (id) => axiosInstance.delete(`/products/${id}`),
};
