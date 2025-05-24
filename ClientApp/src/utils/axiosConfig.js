import axios from 'axios';

// Tạo một instance của axios
const api = axios.create({
  baseURL: '/api', // Sau này sẽ thay đổi URL API thực tế
});

// Thêm interceptor để xử lý JWT token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Xử lý các lỗi phản hồi (401, 403, ...)
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Xử lý lỗi xác thực
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
