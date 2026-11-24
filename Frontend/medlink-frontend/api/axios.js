
import axios from 'axios';

const API = axios.create({
  baseURL: 'https://localhost:7048/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add the authentication token automatically to requests.
API.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const url = (config.url || '').toLowerCase();

    const isAuthCall =
      url.includes('/account/login') ||
      url.includes('/account/register');

    const token = localStorage.getItem('token');

    if (token && !isAuthCall) {
      config.headers.Authorization = `Bearer ${token}`;
    }
  }

  return config;
});

export default API;
