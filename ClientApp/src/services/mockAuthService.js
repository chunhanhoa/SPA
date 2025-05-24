// Service giả để xử lý xác thực trong quá trình phát triển frontend
const mockUsers = [
  { id: 1, username: 'admin', password: 'admin123', role: 'admin' },
  { id: 2, username: 'user', password: 'user123', role: 'user' }
];

export const mockAuthService = {
  login: (username, password) => {
    const user = mockUsers.find(u => u.username === username && u.password === password);
    if (user) {
      // Giả lập JWT token
      const token = `mock_jwt_token_for_${username}_${new Date().getTime()}`;
      localStorage.setItem('token', token);
      localStorage.setItem('user', JSON.stringify(user));
      return Promise.resolve({ success: true, token, user });
    }
    return Promise.reject({ success: false, message: 'Tên đăng nhập hoặc mật khẩu không chính xác' });
  },
  
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    return Promise.resolve(true);
  },
  
  getCurrentUser: () => {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  },
  
  isAuthenticated: () => {
    return !!localStorage.getItem('token');
  }
};
