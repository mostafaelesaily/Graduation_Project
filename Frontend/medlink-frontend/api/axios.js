// import axios from 'axios';

// const API = axios.create({
//   baseURL: 'https://localhost:44302/api',
//   headers: {
//     'Content-Type': 'application/json',
//   },
// });

// export default API;
import axios from 'axios';

// Point the frontend at whichever backend URL you are running.
//   IIS Express (default) : https://localhost:44302/api
//   dotnet run (http)     : http://localhost:5031/api
//   dotnet run (https)    : https://localhost:7048/api
// Set NEXT_PUBLIC_API_URL in .env.local to override the default.
export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || 'https://localhost:44302/api';

const API = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});
export default API;


export default API;
