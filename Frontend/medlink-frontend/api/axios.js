
import axios from 'axios';

// Backend URL can be overridden in .env.local
// IIS Express: https://localhost:44302/api
// dotnet run (http): http://localhost:5031/api
// dotnet run (https): https://localhost:7048/api
export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || 'https://localhost:7048/api';

const API = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});
export default API;


// Add authentication token automatically to requests.
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
