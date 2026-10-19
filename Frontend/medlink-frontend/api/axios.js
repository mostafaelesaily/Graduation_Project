import axios from 'axios';

const API = axios.create({
  baseURL: 'https://localhost:7048/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default API;