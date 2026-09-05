import { create } from 'zustand';

export const useAppStore = create((set) => ({
  notification: null,
  setNotification: (notification) => set({ notification }),
  clearNotification: () => set({ notification: null }),
}));
